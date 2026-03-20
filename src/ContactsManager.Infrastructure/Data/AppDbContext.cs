using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
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
}
