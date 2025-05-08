using TVDataHub.Domain.Dto;

namespace TVDataHub.Domain.Scraper;

public interface ITVMazeScraperService
{
    Task<IReadOnlyList<TVMazeTVShowDto>> GetPaginatedTVShowAsync(int page);

    Task<TVMazeTVShowDto?> GetTVShowAsync(int id);

    Task<IReadOnlyList<TVMazeCastDto>> GetTVShowCastMembersAsync(int tvShowId);

    Task<Dictionary<int, long>> GetShowUpdatesAsync();
}
