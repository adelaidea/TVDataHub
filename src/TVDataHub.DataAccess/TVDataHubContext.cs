using Microsoft.EntityFrameworkCore;
using TVDataHub.DataAccess.Configuration;
using TVDataHub.Domain.Entity;

namespace TVDataHub.DataAccess;

public sealed class TVDataHubContext(DbContextOptions<TVDataHubContext> options) : DbContext(options)
{
    public DbSet<CastMember> CastMembers => Set<CastMember>();
    
    public DbSet<TVShow> TVShows => Set<TVShow>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CastMemberConfiguration());
        modelBuilder.ApplyConfiguration(new TVShowConfiguration());
    }
}