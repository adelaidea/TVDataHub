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
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var persons = await UpsertPerson(tvShow);

        await UpsertTVShow(tvShow, persons);

        await transaction.CommitAsync();
    }

    private async Task UpsertTVShow(TVShow tvShow, List<Person> persons)
    {
        var existingShow = await dbContext.TVShows
            .Include(s => s.Cast)
            .FirstOrDefaultAsync(s => s.Id == tvShow.Id)
            .ConfigureAwait(false);

        if (existingShow is null)
        {
            tvShow.Cast = persons;
            dbContext.TVShows.Add(tvShow);
        }
        else
        {
            existingShow.Name = tvShow.Name;
            existingShow.Premiered = tvShow.Premiered;
            existingShow.Ended = tvShow.Ended;
            existingShow.Updated = tvShow.Updated;
            existingShow.Genres = tvShow.Genres;

            existingShow.Cast.Clear();
            foreach (var p in persons)
            {
                existingShow.Cast.Add(p);
            }
        }

        await dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    private async Task<List<Person>> UpsertPerson(TVShow tvShow)
    {
        var trackedPersons = new List<Person>();

        foreach (var castMember in tvShow.Cast)
        {
            var existingPerson = await dbContext.Persons
                .FirstOrDefaultAsync(p => p.Id == castMember.Id)
                .ConfigureAwait(false);

            if (existingPerson == null)
            {
                dbContext.Persons.Add(castMember);
                trackedPersons.Add(castMember);
            }
            else
            {
                existingPerson.Name = castMember.Name;
                existingPerson.Birthday = castMember.Birthday;
                existingPerson.Deathday = castMember.Deathday;
                existingPerson.Gender = castMember.Gender;
                trackedPersons.Add(existingPerson);
            }
        }

        await dbContext.SaveChangesAsync()
            .ConfigureAwait(false);

        return trackedPersons;
    }

    public async Task UpsertTVShows(IEnumerable<TVShow> tvShows)
    {
        await using var transaction = await dbContext.Database.BeginTransactionAsync();

        var tvShowIds = tvShows.Select(s => s.Id).ToList();

        var existingTVShows = await dbContext.TVShows
            .Where(s => tvShowIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id);

        foreach (var tvShow in tvShows)
        {
            if (existingTVShows.TryGetValue(tvShow.Id, out var existing))
            {
                dbContext.Entry(existing).CurrentValues.SetValues(tvShow);
            }
            else
            {
                await dbContext.TVShows.AddAsync(tvShow);
            }
        }

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
    }

    public async Task<IReadOnlyCollection<TVShow>> GetPaginated(int pageSize, int page = 1) =>
        await dbContext.TVShows
            .AsNoTracking()
            .OrderBy(s => s.Id)
            .Include(s => s.Cast)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> GetTotal() =>
        await dbContext.TVShows
            .AsNoTracking()
            .CountAsync();

    public async Task<Dictionary<int, long>> GetLastUpdatedMoment() =>
        await dbContext.TVShows
            .AsNoTracking()
            .Select(s => new { s.Id, s.Updated })
            .ToDictionaryAsync(x => x.Id, x => x.Updated);
}