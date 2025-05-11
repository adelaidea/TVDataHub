using TVDataHub.Core.UseCase;

namespace TVDataHub.Api.Jobs;

public class SyncTVShowsToBeUpsertedJob(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SyncTVShowsToBeUpsertedJob> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(12);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var tvShowsToBeUpsertedUseCase = scope.ServiceProvider.GetRequiredService<IGetTVShowsToBeUpsertedUseCase>();
            
            logger.LogInformation("Start syncing TVShow data;");

            await tvShowsToBeUpsertedUseCase.ExecuteAsync();
            
            logger.LogInformation("Finished syncing TVShow data.");
            
            // Wait for the next schedule
            await Task.Delay(_interval, stoppingToken);
        }
    }
}