using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace TVDataHub.DataAccess.Tests.Acceptance;

public class TestBase : IAsyncLifetime, IAsyncDisposable
{
    private readonly PostgreSqlContainer _postgreSqlContainer;

    public TVDataHubContext DbContext { get; private set; } = null!;

    public TestBase()
    {
        _postgreSqlContainer = new PostgreSqlBuilder()
            .WithImage("postgres:16")
            .WithUsername("rootest")
            .WithPassword("rootest")
            .WithDatabase("tvdatahub_test")
            .WithCleanUp(true)
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var options = new DbContextOptionsBuilder<TVDataHubContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString())
            .Options;

        DbContext = new TVDataHubContext(options);
        await DbContext.Database.EnsureCreatedAsync();
    }
    
    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }

    ValueTask IAsyncDisposable.DisposeAsync() => 
        ValueTask.CompletedTask;
}