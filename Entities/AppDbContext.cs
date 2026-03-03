using System;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Entities;

public class AppDbContext : DbContext
{
    public virtual DbSet<Person> Persons { get; set; }
    public virtual DbSet<Country> Countries { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Country>(c =>
        {
            c.ToTable("Countries");

            c.HasKey(c => c.Id);
            c.Property(c => c.Id).ValueGeneratedNever();

            c.HasIndex(c => c.Name).IsUnique();

            c.Property(c => c.Name)
                .HasMaxLength(100)
                .IsRequired();

            // var countries = GetJsonFileData<Country>("countries.json");

            // c.HasData(countries);
        });

        modelBuilder.Entity<Person>(p =>
        {
            p.ToTable("Persons");

            p.HasKey(p => p.Id);
            p.Property(p => p.Id).ValueGeneratedNever();

            p.Property(p => p.Name)
                .HasMaxLength(40)
                .IsRequired();

            p.Property(p => p.Email)
                .HasMaxLength(200)
                .IsRequired();

            p.HasIndex(p => p.Email).IsUnique();

            p.Property(p => p.Address)
                .HasMaxLength(255)
                .IsRequired(false);

            p.Property(p => p.Gender)
                .HasMaxLength(10)
                .IsRequired();

            p.Property(p => p.ReceiveNewsLetters)
                .IsRequired();

            p.Property(p => p.DateOfBirth)
                .HasColumnType("datetime2")
                .IsRequired(false);

            p.HasOne(p => p.Country)
                .WithMany(c => c.Persons)
                .HasForeignKey(c => c.CountryId)
                .IsRequired();

            p.Property(p => p.Tin)
                .HasColumnName("TaxIdentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("ABC12345");

            // p.HasIndex(p => p.Tin).IsUnique();

            p.ToTable(t =>
            {
                t.HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");
            });

            // var persons = GetJsonFileData<Person>("persons.json");
            // p.HasData(persons);
        });
    }

    public IEnumerable<Person> Sp_GetAllPersons()
    {
        return Persons.FromSql($"EXEC [dbo].[GetAllPersons]").ToList();
    }

    public int SP_InsertPerson(Person person)
    {
        SqlParameter[] parameters =
        {
            new SqlParameter("@Id",person.Id),
            new SqlParameter("@Name",person.Name),
            new SqlParameter("@Email",person.Email),
            new SqlParameter("@Gender",person.Gender),
            new SqlParameter("@Address",person.Address),
            new SqlParameter("@CountryId",person.CountryId),
            new SqlParameter("@DateOfBirth",person.DateOfBirth),
            new SqlParameter("@ReceiveNewsLetters",person.ReceiveNewsLetters),
        };

        return Database.ExecuteSqlRaw($"Execute [dbo].[InsertPerson] @Id,@Name,@Email,@Gender,@Address,@ReceiveNewsLetters,@CountryId,@DateOfBirth", parameters);
    }

    private List<T> GetJsonFileData<T>(string jsonFile)
    {
        var path = Path.Combine(AppContext.BaseDirectory, jsonFile);
        if (!File.Exists(path)) return new List<T>();

        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<List<T>>(json);
        return data ?? new List<T>();
    }
}
