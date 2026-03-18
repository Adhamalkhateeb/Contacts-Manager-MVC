using ContactsManager.Domain.Countries;
using ContactsManager.Domain.Persons;
using Microsoft.EntityFrameworkCore;

namespace ContactsManager.Application.Common.Interfaces;

public interface IAppDbContext
{
    public DbSet<Country> Countries { get; }
    public DbSet<Person> Persons { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
