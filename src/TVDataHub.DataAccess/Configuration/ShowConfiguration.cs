using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TVDataHub.Domain.Entity;

namespace TVDataHub.DataAccess.Configuration;

public sealed class ShowConfiguration : IEntityTypeConfiguration<Show>
{
    public void Configure(EntityTypeBuilder<Show> builder)
    {
        builder.ToTable("Show");

        builder.HasKey(s => s.Id);
        
        builder
            .Property(s => s.Id)
            .HasColumnName("Id");
        
        builder
            .Property(s => s.Name)
            .HasColumnName("Name");

        builder
            .HasMany(s => s.Cast)
            .WithOne(c => c.Show)
            .HasForeignKey(c => c.ShowId);
    }
}