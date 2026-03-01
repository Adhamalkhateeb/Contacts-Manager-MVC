using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoriesContract;

namespace Repositories;

public class CountriesRepository : ICountriesRepository
{

    private readonly AppDbContext _context;

    public CountriesRepository(AppDbContext context)
    {
        _context = context;
    }
    public async Task<Country> AddAsync(Country country)
    {
        _context.Countries.Add(country);
        await _context.SaveChangesAsync();

        return country;
    }

    public async Task<IEnumerable<Country>> GetAllAsync()
    {
        return await _context.Countries
                .AsNoTracking()
                .ToListAsync();
    }

    public async Task<Country?> GetByIdAsync(Guid countryId)
    {
        return await _context.Countries.FindAsync(countryId);
    }

    public async Task<Country?> GetByName(string countryName)
    {
        return await _context.Countries
        .FirstOrDefaultAsync(c =>
            c.Name != null &&
            c.Name.ToLower() == countryName.ToLower()
        );
    }
}
