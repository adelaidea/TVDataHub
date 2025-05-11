using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TVDataHub.Core.Domain.Entity;

namespace TVDataHub.DataAccess.Configuration;

public sealed class TVShowConfiguration : IEntityTypeConfiguration<TVShow>
{
    public void Configure(EntityTypeBuilder<TVShow> builder)
    {
        builder.ToTable("TVShows");

        builder.HasKey(s => s.Id);
        
        builder
            .Property(s => s.Id)
            .HasColumnName("Id");
        
        builder
            .Property(s => s.Name)
            .HasColumnName("Name");

        builder
            .HasMany(s => s.Cast)
            .WithOne(c => c.TVShow)
            .HasForeignKey(c => c.TVShowId);
    }
}