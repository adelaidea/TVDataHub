using Microsoft.EntityFrameworkCore;
using TVDataHub.Core.Domain.Entity;
using TVDataHub.DataAccess.Configuration;

namespace TVDataHub.DataAccess;

public sealed class TVDataHubContext(DbContextOptions<TVDataHubContext> options) : DbContext(options)
{
    public DbSet<Person> Persons => Set<Person>();
    
    public DbSet<TVShow> TVShows => Set<TVShow>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new PersonConfiguration());
        modelBuilder.ApplyConfiguration(new TVShowConfiguration());
    }
}