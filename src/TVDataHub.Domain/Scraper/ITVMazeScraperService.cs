using TVDataHub.Domain.Dto;

namespace TVDataHub.Domain.Scraper;

public interface ITVMazeScraperService
{
    Task<IReadOnlyList<TVMazeShowDto>> GetPaginatedTVShowAsync(int page);

    Task<TVMazeShowWithCastDto?> GetTVShowAsync(int id);

    Task<IReadOnlyList<TVMazeCastDto>> GetTVShowCastMembersAsync(int tvShowId);

    Task<Dictionary<int, long>> GetTVShowDailyUpdatesAsync();
}
