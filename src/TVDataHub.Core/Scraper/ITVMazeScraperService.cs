using TVDataHub.Core.Scraper.Dto;

namespace TVDataHub.Core.Scraper;

public interface ITVMazeScraperService
{
    Task<IReadOnlyList<TVMazeShowDto>> GetPaginatedTVShowAsync(int page);

    Task<TVMazeShowDto?> GetTVShowAsync(int id);

    Task<IReadOnlyList<TVMazeCastDto>> GetTVShowCastMembersAsync(int tvShowId);

    Task<Dictionary<int, long>> GetTVShowDailyUpdatesAsync();
}