using Microsoft.EntityFrameworkCore;
using TVDataHub.Core.Domain.Entity;
using TVDataHub.Core.Repository;

namespace TVDataHub.DataAccess.Repository;

public class TVShowRepository(
    TVDataHubContext dbContext) : ITVShowRepository
{
    public async Task UpsertTVShow(TVShow tvShow)
    {
        var existingTVShow = await dbContext.TVShows
            .FirstOrDefaultAsync(s => s.Id == tvShow.Id);

        if (existingTVShow == null)
        {
            await dbContext.TVShows.AddAsync(tvShow);
        }
        else
        {
            dbContext.Entry(existingTVShow).CurrentValues.SetValues(tvShow);
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task UpsertTVShowWithCast(TVShow tvShow)
    {
        try
        {
            foreach (var cast in tvShow.Cast)
            {
                cast.TVShowId = tvShow.Id;
            }

            await using var transaction = await dbContext.Database.BeginTransactionAsync();

            var existingShow = await dbContext.TVShows
                .AsNoTracking()
                .AnyAsync(s => s.Id == tvShow.Id)
                .ConfigureAwait(false);

            if (!existingShow)
            {
                dbContext.TVShows.Add(tvShow);
            }
            else
            {
                await dbContext.TVShows
                    .Where(s => s.Id == tvShow.Id)
                    .ExecuteUpdateAsync(u => u
                            .SetProperty(p => p.Name, p => tvShow.Name)
                            .SetProperty(p => p.Updated, p => tvShow.Updated)
                            .SetProperty(p => p.Premiered, p => tvShow.Premiered)
                            .SetProperty(p => p.Ended, p => tvShow.Ended)
                            .SetProperty(p => p.Genres, p => tvShow.Genres))
                    .ConfigureAwait(false);

                await dbContext.CastMembers
                    .Where(c => c.TVShowId == tvShow.Id)
                    .ExecuteDeleteAsync()
                    .ConfigureAwait(false);

                dbContext.ChangeTracker.Clear();

                await dbContext.CastMembers
                    .AddRangeAsync(tvShow.Cast)
                    .ConfigureAwait(false);
            }

            await dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public async Task UpsertTVShows(IEnumerable<TVShow> tvShows)
    {
        var tvShowIds = tvShows.Select(s => s.Id).ToList();

        var existingTVShows = await dbContext.TVShows
            .Where(s => tvShowIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        foreach (var tvShow in existingTVShows)
        {
            if (existingTVShows.TryGetValue(tvShow.Key, out var existing))
            {
                dbContext.Entry(existing).CurrentValues.SetValues(tvShow.Value);
            }
            else
            {
                await dbContext.TVShows.AddAsync(tvShow.Value);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<IReadOnlyCollection<TVShow>> GetPaginated(int pageSize, int page = 1) =>
        await dbContext.TVShows
            .Include(s => s.Cast)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetLastId() =>
        await dbContext.TVShows
            .OrderByDescending(s => s.Id)
            .Select(s => s.Id)
            .FirstOrDefaultAsync();

    public async Task<Dictionary<int, long>> GetLastUpdatedMoment(int[] ids) =>
        await dbContext.TVShows
            .AsNoTracking()
            .Where(s => ids.Contains(s.Id))
            .Select(s => new { s.Id, s.Updated })
            .ToDictionaryAsync(x => x.Id, x => x.Updated);
}