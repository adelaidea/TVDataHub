using TVDataHub.Core.Scraper.Dto;

namespace TVDataHub.Core.Scraper;

public interface ITVMazeScraperService
{
    Task<TVMazeShowDto?> GetTVShowAsync(int id);

    Task<Dictionary<int, long>> GetTVShowUpdatesAsync();
}