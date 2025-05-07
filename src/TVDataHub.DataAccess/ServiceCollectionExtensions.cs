using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TVDataHub.DataAccess.Repository;
using TVDataHub.Domain.Repository;

namespace TVDataHub.DataAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TVDataHubContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PostgresConnection")));

        services.AddScoped<IShowRepository, ShowRepository>();
        services.AddScoped<ICastMemberRepository, CastMemberRepository>();

        return services;
    }
}