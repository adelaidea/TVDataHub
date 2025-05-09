using TVDataHub.Application.UseCase;

namespace TVDataHub.Api.Jobs;

public class SyncUpdatedTVShowsJob(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SyncUpdatedTVShowsJob> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromHours(12);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var getAndUpdateTVShowsUseCase = scope.ServiceProvider.GetRequiredService<IGetAndUpdateTVShowsUseCase>();
            
            logger.LogInformation("Start syncing updated TVShow data;");

            await getAndUpdateTVShowsUseCase.ExecuteAsync();
            
            logger.LogInformation("Finished syncing updated TVShow data.");
            
            // Wait for the next schedule
            await Task.Delay(_interval, stoppingToken);
        }
    }
}