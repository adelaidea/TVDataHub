using Microsoft.EntityFrameworkCore;
using TVDataHub.Domain.Entity;
using TVDataHub.Domain.Repository;

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
        foreach (var castMember in tvShow.Cast)
        {
            castMember.TVShowId = tvShow.Id;
        }
        
        var existingShow = await dbContext.TVShows
            .Include(s => s.Cast)
            .FirstOrDefaultAsync(s => s.Id == tvShow.Id);

        if (existingShow is null)
        {
            dbContext.TVShows.Add(tvShow);
        }
        else
        {
            dbContext.Entry(existingShow).CurrentValues.SetValues(tvShow);

            dbContext.CastMembers.RemoveRange(existingShow.Cast);

            foreach (var cast in tvShow.Cast)
            {
                existingShow.Cast.Add(new CastMember
                {
                    Name = cast.Name,
                    TVShowId = existingShow.Id
                });
            }
        }

        await dbContext.SaveChangesAsync();
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