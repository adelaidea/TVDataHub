using Microsoft.Extensions.DependencyInjection;
using TVDataHub.Application.UseCase;

namespace TVDataHub.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUseCases(
        this IServiceCollection services)
    {
        services.AddScoped<IGetPaginatedTVShowsUseCase, GetPaginatedTVShowsUseCase>();

        return services;
    }
}