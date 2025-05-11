using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TVDataHub.Core.Domain.Entity;

namespace TVDataHub.DataAccess.Configuration;

public sealed class TVShowConfiguration : IEntityTypeConfiguration<TVShow>
{
    public void Configure(EntityTypeBuilder<TVShow> builder)
    {
        builder.ToTable("TVShows");

        builder.HasKey(show => show.Id);

        builder.Property(show => show.Name)
            .IsRequired();

        builder.Property(show => show.Updated)
            .IsRequired();
        
        builder.Property(show => show.Premiered)
            .IsRequired(false);

        builder.Property(show => show.Ended)
            .IsRequired(false);

        builder.Property(show => show.Genres)
            .HasConversion(
                v => string.Join(',', v), 
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        
        builder
            .HasMany(show => show.Cast)
            .WithMany(person => person.TVShows)
            .UsingEntity(j => j.ToTable("TVShowPerson"));
    }
}