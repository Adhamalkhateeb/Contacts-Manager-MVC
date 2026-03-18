using System;
using ContactsManager.Domain.Countries;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ContactsManager.Infrastructure.Data.Configurations;

public class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> c)
    {
        c.ToTable("Countries");

        c.HasKey(c => c.Id);
        c.Property(c => c.Id).ValueGeneratedNever();

        c.HasIndex(c => c.Name).IsUnique();

        c.Property(c => c.Name).HasMaxLength(100).IsRequired();

        // var countries = GetJsonFileData<Country>("countries.json");

        // c.HasData(countries);
    }
}
