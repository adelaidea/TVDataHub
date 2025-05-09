using TVDataHub.Domain.Entity;

namespace TVDataHub.Domain.Repository;

public interface ITVShowRepository
{
    Task UpsertTVShow(TVShow tvShow);

    Task UpsertTVShowWithCast(TVShow tvShow);

    Task UpsertTVShows(IEnumerable<TVShow> tvShows);

    Task<IReadOnlyCollection<TVShow>> GetPaginated(int pageSize, int page = 1);

    Task<int> GetLastId();

    Task<Dictionary<int, long>> GetLastUpdatedMoment(int[] ids);
}