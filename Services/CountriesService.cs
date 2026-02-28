using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{
    private readonly ContactsManagerDbContext _context;

    public CountriesService(ContactsManagerDbContext context)
    {
        _context = context;
    }
    public async Task<CountryResponse> AddAsync(CountryAddRequest? countryAddRequest)
    {
        ArgumentNullException.ThrowIfNull(countryAddRequest);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryAddRequest.Name);


        if (await _context.Countries.AnyAsync(c =>
            c.Name!.Equals(countryAddRequest.Name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new ArgumentException("Country already exists");
        }

        var country = countryAddRequest.ToCountry();
        country.Id = Guid.NewGuid();

        await _context.Countries.AddAsync(country);
        await _context.SaveChangesAsync();

        return country.ToCountryResponse();

    }

    public async Task<List<CountryResponse>> GetAllAsync()
    {
        var countries = await _context.Countries
        .AsNoTracking()
        .ToListAsync();

        return countries.Select(c => c.ToCountryResponse()).ToList();
    }

    public async Task<CountryResponse?> GetByIdAsync(Guid? id)
    {
        if (id is null)
            return null;

        var country = await _context.Countries.FindAsync(id);
        return country?.ToCountryResponse();
    }

    public async Task<int> UploadFromExcelFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file.");

        var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var excel = new ExcelPackage(memoryStream);
        var worksheet = excel.Workbook.Worksheets["Countries"];

        if (worksheet == null)
            throw new InvalidOperationException("Worksheet 'Countries' not found.");

        if (worksheet.Dimension == null)
            return 0;

        int rowsCount = worksheet.Dimension.Rows;

        var countryNamesFromFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int row = 2; row <= rowsCount; row++)
        {
            var countryName = worksheet.Cells[row, 1].Value?.ToString()?.Trim();

            if (!string.IsNullOrWhiteSpace(countryName))
                countryNamesFromFile.Add(countryName);
        }

        if (!countryNamesFromFile.Any())
            return 0;

        var existingCountries = await _context.Countries
            .Where(c => c.Name != null && countryNamesFromFile.Contains(c.Name))
            .Select(c => c.Name!)
            .ToListAsync();

        var newCountries = countryNamesFromFile
            .Except(existingCountries, StringComparer.OrdinalIgnoreCase)
            .Select(name => new Country { Id = Guid.NewGuid(), Name = name })
            .ToList();

        await _context.Countries.AddRangeAsync(newCountries);

        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException("Database update failed. Possible duplicate data.");
        }
    }
}
