using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TVDataHub.Core.Domain.Entity;

namespace TVDataHub.DataAccess.Configuration;

public sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> builder)
    {
        builder.ToTable("Persons");

        builder.Property(person => person.Name)
            .IsRequired();

        builder.Property(person => person.Birthday)
            .IsRequired(false);
    }
}