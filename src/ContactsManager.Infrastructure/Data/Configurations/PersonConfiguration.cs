using System;
using ContactsManager.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace ContactsManager.Infrastructure.Data.Configurations;

public class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(
        Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Person> p
    )
    {
        p.ToTable("Persons");

        p.HasKey(p => p.Id);
        p.Property(p => p.Id).ValueGeneratedNever();

        p.Property(p => p.Name).HasMaxLength(100).IsRequired();

        p.Property(p => p.Email).HasMaxLength(255).IsRequired();

        p.HasIndex(p => p.Email).IsUnique();

        p.Property(p => p.Address).HasMaxLength(500).IsRequired(false);

        p.Property(p => p.Gender).IsRequired();

        p.Property(p => p.ReceiveNewsLetters).IsRequired();

        p.Property(p => p.DateOfBirth).HasColumnType("datetime2").IsRequired(false);

        p.HasOne(p => p.Country).WithMany().HasForeignKey(c => c.CountryId).IsRequired();

        // var persons = GetJsonFileData<Person>("persons.json");
        // p.HasData(persons);
    }
}
