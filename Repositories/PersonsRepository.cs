using System;
using System.Linq.Expressions;
using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoriesContract;

namespace Repositories;

public class PersonsRepository : IPersonsRepository
{

    private readonly AppDbContext _context;

    public PersonsRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Person> AddAsync(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        _context.Persons.Add(person);
        await _context.SaveChangesAsync();

        return person;
    }

    public async Task<int> DeleteAsync(Guid personId)
    {
        return await _context.Persons
                .Where(p => p.Id == personId)
                .ExecuteDeleteAsync();
    }

    public async Task<IEnumerable<Person>> GetAllAsync()
    {

        return await _context.Persons
                        .AsNoTracking()
                        .Include(p => p.Country)
                        .ToListAsync();
    }

    public async Task<Person?> GetById(Guid personId)
    {
        return await _context.Persons
                    .Include(p => p.Country)
                    .FirstOrDefaultAsync(p => p.Id == personId);
    }

    public async Task<IEnumerable<Person>> GetFilteredAsync(Expression<Func<Person, bool>> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);

        return await _context.Persons
                        .AsNoTracking()
                        .Where(predicate)
                        .Include(p => p.Country)
                        .ToListAsync();
    }

    public async Task<Person?> UpdateAsync(Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        var existingPerson = await _context.Persons
                    .FirstOrDefaultAsync(p => p.Id == person.Id);

        if (existingPerson is null)
            return null;

        _context.Entry(existingPerson)
                .CurrentValues
                .SetValues(person);

        await _context.SaveChangesAsync();

        return existingPerson;
    }
}
