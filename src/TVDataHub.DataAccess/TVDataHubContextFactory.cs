using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TVDataHub.DataAccess;

public class TVDataHubContextFactory : IDesignTimeDbContextFactory<TVDataHubContext>
{
    public TVDataHubContext CreateDbContext(string[] args)
    {
        string connectionString = ReadDefaultConnectionStringFromAppSettings();

        DbContextOptionsBuilder<TVDataHubContext> builder = new DbContextOptionsBuilder<TVDataHubContext>();
        builder.UseNpgsql(connectionString);
        
        return new TVDataHubContext(builder.Options);
    }
    
    private static string ReadDefaultConnectionStringFromAppSettings()
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false)
            .Build();

        string connectionString = configuration.GetConnectionString("PostgresConnection")!;
        return connectionString;
    }
}