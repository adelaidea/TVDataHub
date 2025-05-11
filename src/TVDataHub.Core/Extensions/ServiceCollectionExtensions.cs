using Microsoft.Extensions.DependencyInjection;
using TVDataHub.Core.UseCase;

namespace TVDataHub.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(
        this IServiceCollection services)
    {
        services.AddScoped<IGetPaginatedTVShowsUseCase, GetPaginatedTVShowsUseCase>();
        services.AddScoped<IUpsertTVShowUseCase, UpsertTVShowUseCase>();
        services.AddScoped<IGetTVShowsToBeUpsertedUseCase, GetTVShowsToBeUpsertedUseCase>();

        return services;
    }
}