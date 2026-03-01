using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using OfficeOpenXml;
using RepositoriesContract;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services;

public class CountriesService : ICountriesService
{

    private readonly ICountriesRepository _countriesRepository;

    public CountriesService(ICountriesRepository countriesRepository)
    {
        _countriesRepository = countriesRepository;
    }
    public async Task<CountryResponse> AddAsync(CountryAddRequest? countryAddRequest)
    {
        ArgumentNullException.ThrowIfNull(countryAddRequest);
        ArgumentException.ThrowIfNullOrWhiteSpace(countryAddRequest.Name);


        var existingCountry = await _countriesRepository
            .GetByName(countryAddRequest.Name);

        if (existingCountry != null)
            throw new ArgumentException("Country already exists.");

        var country = countryAddRequest.ToCountry();
        country.Id = Guid.NewGuid();

        country = await _countriesRepository.AddAsync(country);
        return country.ToCountryResponse();

    }

    public async Task<List<CountryResponse>> GetAllAsync()
    {
        return (await _countriesRepository.GetAllAsync())
            .Select(c => c.ToCountryResponse())
            .ToList();
    }

    public async Task<CountryResponse?> GetByIdAsync(Guid? id)
    {
        if (id == null)
            return null;

        var country = await _countriesRepository.GetByIdAsync(id.Value);

        return country?.ToCountryResponse();
    }

    public async Task<int> UploadFromExcelFileAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Invalid file.");

        using var memoryStream = new MemoryStream();
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

        var existingCountries = await _countriesRepository.GetAllAsync();

        var existingNames = existingCountries
            .Where(c => c.Name != null)
            .Select(c => c.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newCountries = countryNamesFromFile
            .Except(existingNames, StringComparer.OrdinalIgnoreCase)
            .Select(name => new Country
            {
                Id = Guid.NewGuid(),
                Name = name
            })
            .ToList();

        if (!newCountries.Any())
            return 0;

        int insertedCount = 0;

        foreach (var country in newCountries)
        {
            await _countriesRepository.AddAsync(country);
            insertedCount++;
        }

        return insertedCount;
    }
}
