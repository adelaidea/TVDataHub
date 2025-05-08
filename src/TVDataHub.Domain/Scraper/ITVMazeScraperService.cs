using TVDataHub.Domain.Dto;

namespace TVDataHub.Domain.Scraper;

public interface ITVMazeScraperService
{
    Task<IReadOnlyList<TVMazeTVShowDto>> GetPaginatedTVShow(int page);

    Task<TVMazeTVShowDto?> GetTVShow(int id);

    Task<IReadOnlyList<TVMazeCastDto>> GetTVShowCastMembers(int tvShowId);
}
