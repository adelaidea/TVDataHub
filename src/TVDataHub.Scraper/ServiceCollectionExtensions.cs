using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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
        
        AddHttpClient(services);
        
        return services;
    }

    private static void AddHttpClient(IServiceCollection services)
    {
        services.AddHttpClient<ITVMazeScraperService, TVMazeScraperService>((sp, client) =>
        {
            var settings = sp.GetRequiredService<IOptions<TVMazeSettings>>().Value;

            client.BaseAddress = new Uri(settings.BaseApi);
        });
    }
}