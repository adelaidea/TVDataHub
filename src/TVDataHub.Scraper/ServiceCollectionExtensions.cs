using System.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;
using TVDataHub.Domain.Scraper;
using TVDataHub.Scraper.Settings;

namespace TVDataHub.Scraper;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScraper(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TVMazeSettings>(configuration.GetSection("TVMazeSettings"));

        services.AddLogging();

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
            .AddPolicyHandler(GetRetryPolicy(sp => sp.GetRequiredService<ILogger<ITVMazeScraperService>>()));
    }

    static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(
        Func<IServiceProvider, ILogger<ITVMazeScraperService>> loggerFactory)
    {
        var logger = loggerFactory.Invoke(null);

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