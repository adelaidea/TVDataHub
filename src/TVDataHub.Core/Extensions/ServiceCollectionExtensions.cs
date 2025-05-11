using Microsoft.Extensions.DependencyInjection;
using TVDataHub.Application.Queues;
using TVDataHub.Core.Types;
using TVDataHub.Core.UseCase;

namespace TVDataHub.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(
        this IServiceCollection services)
    {
        services.AddSingleton<IStaticQueue<TVShowId>, StaticQueue<TVShowId>>();

        services.AddScoped<IGetPaginatedTVShowsUseCase, GetPaginatedTVShowsUseCase>();
        services.AddScoped<IUpsertTVShowUseCase, UpsertTVShowUseCase>();
        services.AddScoped<IGetTVShowsToBeUpsertedUseCase, GetTVShowsToBeUpsertedUseCase>();

        return services;
    }
}