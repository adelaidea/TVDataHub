using TVDataHub.Application.UseCase;

namespace TVDataHub.Api.Jobs;

public class SyncTVShowsCastJob(
    IServiceScopeFactory serviceScopeFactory,
    ILogger<SyncTVShowsCastJob> logger) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromDays(1);
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = serviceScopeFactory.CreateScope();
            var getAndInsertTVShowsCast = scope.ServiceProvider.GetRequiredService<IGetAndInsertTVShowsCast>();
            
            logger.LogInformation("Start syncing TVShow Cast data;");

            await getAndInsertTVShowsCast.ExecuteAsync();
            
            logger.LogInformation("Finished syncing TVShow Cast data.");
            
            // Wait for the next schedule
            await Task.Delay(_interval, stoppingToken);
        }
    }
}