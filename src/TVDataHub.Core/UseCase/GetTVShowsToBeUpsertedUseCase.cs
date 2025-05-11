using Microsoft.Extensions.Logging;
using TVDataHub.Application.Queues;
using TVDataHub.Core.Repository;
using TVDataHub.Core.Scraper;
using TVDataHub.Core.Types;

namespace TVDataHub.Core.UseCase;

public interface IGetTVShowsToBeUpsertedUseCase
{
    Task ExecuteAsync();
}

internal sealed class GetTVShowsToBeUpsertedUseCase(
    ITVMazeScraperService tvMazeScraperService,
    ITVShowRepository tvShowRepository,
    IStaticQueue<TVShowId> tvShowQueue,
    ILogger<GetTVShowsToBeUpsertedUseCase> logger) : IGetTVShowsToBeUpsertedUseCase
{
    public async Task ExecuteAsync()
    {
        logger.LogInformation("TVShow update process started.");

        try
        {
            var startTime = DateTime.UtcNow;

            var remoteUpdates = await tvMazeScraperService.GetTVShowUpdatesAsync();

            var localUpdates = await tvShowRepository.GetLastUpdatedMoment();

            var missingTVShows = remoteUpdates
                .Where(s => !localUpdates.ContainsKey(s.Key))
                .ToDictionary(s => s.Key, s => s.Value);

            logger.LogInformation("Identified {MissingCount} new TVShows.", missingTVShows.Count);

            var outdatedTVShows = remoteUpdates
                .Where(s =>
                    localUpdates.TryGetValue(s.Key, out var savedUpdated) && savedUpdated != s.Value)
                .ToDictionary(s => s.Key, kvp => kvp.Value);
            
            logger.LogInformation("Identified {OutdatedCount} outdated TVShows.", outdatedTVShows.Count);

            var tvShowsToBeUpdated = missingTVShows.Keys
                .Concat(outdatedTVShows.Keys)
                .Select(id => new TVShowId(id))
                .ToList();
            
            logger.LogInformation("Enqueuing {TotalCount} TVShows for update.", tvShowsToBeUpdated.Count);
            
            if(tvShowsToBeUpdated.Count > 0 )
            {
                tvShowQueue.EnqueueMany(tvShowsToBeUpdated);
            }    
            
            var duration = DateTime.UtcNow - startTime;
            logger.LogInformation("TVShow update process completed in {Duration}ms.", duration.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating missing TVShows.");
            throw;
        }
    }
}