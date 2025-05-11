using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using TVDataHub.Core.Repository;
using TVDataHub.Core.Scraper;
using TVDataHub.DataAccess.Repository;
using TVDataHub.DataAccess.Scraper;
using TVDataHub.DataAccess.Settings;

namespace TVDataHub.DataAccess.Extentions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TVMazeSettings>(configuration.GetSection("TVMazeSettings"));
        
        services.AddDbContext<TVDataHubContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));
        
        services.AddScoped<ITVShowRepository, TVShowRepository>();

        services.AddScoped<ITVMazeScraperService, TVMazeScraperService>();
        
        AddHttpClient(services);

        return services;
    }
    
    private static void AddHttpClient(IServiceCollection services)
    {
        services.AddHttpClient<ITVMazeScraperService, TVMazeScraperService>((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<TVMazeSettings>>().Value;
                client.BaseAddress = new Uri(settings.BaseApi);
            })
            .AddPolicyHandler((sp, _) =>
                GetRetryPolicy(sp.GetRequiredService<ILogger<ITVMazeScraperService>>()));
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
        ILogger<ITVMazeScraperService> logger)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                {
                    var jitter = Random.Shared.Next(0, 1000);
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitter);
                },
                onRetry: (result, timespan, retryAttempt, _) =>
                {
                    logger.LogWarning(
                        $"Retry {retryAttempt} due to {result.Result?.StatusCode}. Waiting {timespan.TotalSeconds}s.");
                });
    }
}