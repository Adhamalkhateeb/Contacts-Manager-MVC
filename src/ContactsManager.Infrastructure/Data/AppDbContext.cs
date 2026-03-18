using System.Text.Json;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ContactsManager.Infrastructure.Data;

public class AppDbContext : DbContext, IAppDbContext
{
    public virtual DbSet<Person> Persons { get; set; }
    public virtual DbSet<Country> Countries { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public IEnumerable<Person> Sp_GetAllPersons()
    {
        return Persons.FromSql($"EXEC [dbo].[GetAllPersons]").ToList();
    }

    public int SP_InsertPerson(Person person)
    {
        SqlParameter[] parameters =
        {
            new SqlParameter("@Id", person.Id),
            new SqlParameter("@Name", person.Name),
            new SqlParameter("@Email", person.Email),
            new SqlParameter("@Gender", person.Gender),
            new SqlParameter("@Address", person.Address),
            new SqlParameter("@CountryId", person.CountryId),
            new SqlParameter("@DateOfBirth", person.DateOfBirth),
            new SqlParameter("@ReceiveNewsLetters", person.ReceiveNewsLetters),
        };

        return Database.ExecuteSqlRaw(
            $"Execute [dbo].[InsertPerson] @Id,@Name,@Email,@Gender,@Address,@ReceiveNewsLetters,@CountryId,@DateOfBirth",
            parameters
        );
    }
}
