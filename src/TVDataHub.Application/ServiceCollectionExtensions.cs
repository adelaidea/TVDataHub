using Microsoft.Extensions.DependencyInjection;
using TVDataHub.Application.UseCase;

namespace TVDataHub.Application;

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