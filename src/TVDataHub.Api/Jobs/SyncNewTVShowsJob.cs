using TVDataHub.Core.UseCase;

namespace TVDataHub.Api.Jobs;

public class SyncNewTVShowsJob(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SyncNewTVShowsJob> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromDays(1);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var updateMissingTvShowsUseCase = scope.ServiceProvider.GetRequiredService<IGetAndInsertNewTVShowsUseCase>();
            
            logger.LogInformation("Start syncing TVShow data;");

            await updateMissingTvShowsUseCase.ExecuteAsync();
            
            logger.LogInformation("Finished syncing TVShow data.");
            
            // Wait for the next schedule
            await Task.Delay(_interval, stoppingToken);
        }
    }
}