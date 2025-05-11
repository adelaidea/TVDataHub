using Microsoft.Extensions.DependencyInjection;
using TVDataHub.Core.UseCase;

namespace TVDataHub.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(
        this IServiceCollection services)
    {
        services.AddScoped<IGetPaginatedTVShowsUseCase, GetPaginatedTVShowsUseCase>();
        services.AddScoped<IGetAndInsertTVShowsCast, SyncTVShowsCast>();
        services.AddScoped<IGetAndUpdateTVShowsUseCase, IGetAndUpdateTVShowsUseCase>();
        services.AddScoped<IGetAndInsertNewTVShowsUseCase, SyncNewTVShowsUseCase>();

        return services;
    }
}