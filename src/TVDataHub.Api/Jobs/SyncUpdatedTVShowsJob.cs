using TVDataHub.Application.Queues;
using TVDataHub.Core.Types;
using TVDataHub.Core.UseCase;

namespace TVDataHub.Api.Jobs;

public class SyncUpdatedTVShowsJob(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SyncUpdatedTVShowsJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            TVShowId? showId = await StaticQueue<TVShowId>.DequeueAsync();

            if (showId == null)
            {
                continue;
            }
            
            try
            {
                using var scope = serviceScopeFactory.CreateScope();
                var upsertUseCase = scope.ServiceProvider.GetRequiredService<IUpsertTVShowUseCase>();

                logger.LogDebug("Dequeued TVShowId {ShowId}, starting upsert.", showId.Value);

                await upsertUseCase.ExecuteAsync(showId.Value);

                logger.LogInformation("Successfully upserted TVShowId {ShowId}", showId.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing TVShowId {ShowId}. TVShow will not be re-enqueued.", showId.Value);
            }
        }
    }
}