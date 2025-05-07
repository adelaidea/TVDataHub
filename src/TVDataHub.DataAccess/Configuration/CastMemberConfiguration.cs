using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TVDataHub.Domain.Entity;

namespace TVDataHub.DataAccess.Configuration;

public sealed class CastMemberConfiguration : IEntityTypeConfiguration<CastMember>
{
    public void Configure(EntityTypeBuilder<CastMember> builder)
    {
        builder.ToTable("CastMember");

        builder.HasKey(c => c.Id);
        
        builder
            .Property(c => c.Id)
            .HasColumnName("Id");
        
        builder
            .Property(c => c.Name)
            .HasColumnName("Name");
        
        builder
            .Property(c => c.Birthday)
            .HasColumnName("Birthday");
        
        builder
            .Property(c => c.ShowId)
            .HasColumnName("ShowId");
    }
}