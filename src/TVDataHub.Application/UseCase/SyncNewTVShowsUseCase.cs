using Microsoft.Extensions.Logging;
using TVDataHub.Application.Queues;
using TVDataHub.Domain.Entity;
using TVDataHub.Domain.Repository;
using TVDataHub.Domain.Scraper;

namespace TVDataHub.Application.UseCase;

public interface IGetAndInsertNewTVShowsUseCase
{
    Task ExecuteAsync();
}

internal sealed class SyncNewTVShowsUseCase(
    ITVMazeScraperService TVMazeScraperService,
    ITVShowRepository tvShowRepository,
    ILogger<SyncNewTVShowsUseCase> logger) : IGetAndInsertNewTVShowsUseCase
{
    public async Task ExecuteAsync()
    {
        try
        {
            logger.LogInformation("Starting missing TV TVshows update process.");

            var lastId = await tvShowRepository.GetLastId();
            var page = lastId / 250;

            logger.LogInformation("Last ID: {LastId}, Page: {Page}", lastId, page);

            var tvshowDtos = await TVMazeScraperService.GetPaginatedTVShowAsync(page);

            if (!tvshowDtos.Any())
            {
                logger.LogInformation("No missing TV TVshows found to update.");
                return;
            }

            var tvShows = tvshowDtos.Select(dto => new TVShow
            {
                Id = dto.Id,
                Name = dto.Name,
                Genres = dto.Genres,
                Premiered = dto.Premiered,
                Ended = dto.Ended,
                Updated = dto.Updated
            }).ToList();

            await tvShowRepository.UpsertTVShows(tvShows);

            logger.LogInformation("{Count} TVShows upserted successfully.", tvShows.Count);

            StaticQueue<int>.EnqueueMany(tvShows.Select(s=>s.Id).ToList());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating missing TVShows.");
            throw;
        }
    }
}