using TVDataHub.Core.Domain.Entity;

namespace TVDataHub.Core.Repository;

public interface ITVShowRepository
{
    Task UpsertTVShow(TVShow tvShow);

    Task UpsertTVShowWithCast(TVShow tvShow);

    Task UpsertTVShows(IEnumerable<TVShow> tvShows);

    Task<IReadOnlyCollection<TVShow>> GetPaginated(int pageSize, int page = 1);

    Task<int> GetTotal();

    Task<Dictionary<int, long>> GetLastUpdatedMoment();
}