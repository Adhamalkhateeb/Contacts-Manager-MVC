using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoriesContract;
using Serilog;
using SerilogTimings;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private IPersonsRepository _personsRepository;
    private ICountriesService _countriesService;
    private readonly IDiagnosticContext _diagnosticContext;

    private readonly ILogger<PersonsService> _logger;

    public PersonsService(
        IPersonsRepository personsRepository,
        ICountriesService countriesService,
        ILogger<PersonsService> logger,
        IDiagnosticContext diagnosticContext
    )
    {
        _personsRepository = personsRepository;
        _countriesService = countriesService;
        _logger = logger;
        _diagnosticContext = diagnosticContext;
    }

    public async Task<PersonResponse> AddAsync(PersonAddRequest? personAddRequest)
    {
        _logger.LogInformation("Adding a new person");

        ArgumentNullException.ThrowIfNull(personAddRequest);
        ValidationHelper.ValidateModel(personAddRequest);

        var country = await _countriesService.GetByIdAsync(personAddRequest.CountryId);
        if (country is null)
        {
            _logger.LogWarning(
                "Invalid CountryId {CountryId} while adding person",
                personAddRequest.CountryId
            );
            throw new ArgumentException("Invalid CountryId");
        }

        var person = personAddRequest.ToPerson();
        person.Id = Guid.NewGuid();

        person = await _personsRepository.AddAsync(person);

        _logger.LogInformation("Person created successfully with Id {PersonId}", person.Id);

        return person.ToPersonResponse();
    }

    public async Task<PersonResponse> UpdateAsync(PersonUpdateRequest? request)
    {
        _logger.LogInformation("Updating person {PersonId}", request?.Id);

        ArgumentNullException.ThrowIfNull(request);
        ValidationHelper.ValidateModel(request);

        var existingPerson = await _personsRepository.GetById(request.Id);

        if (existingPerson is null)
        {
            _logger.LogWarning("Update failed. Person {PersonId} not found", request.Id);
            throw new ArgumentException($"Person with Id {request.Id} doesn't exist");
        }

        var country = await _countriesService.GetByIdAsync(request.CountryId);
        if (country is null)
        {
            _logger.LogWarning(
                "Invalid CountryId {CountryId} while Updating person",
                request.CountryId
            );
            throw new ArgumentException("Invalid CountryId");
        }

        var updatedPerson = await _personsRepository.UpdateAsync(request.ToPerson());

        _logger.LogInformation("Person {PersonId} updated successfully", request.Id);

        return updatedPerson!.ToPersonResponse();
    }

    public async Task<bool> DeleteAsync(Guid? personId)
    {
        _logger.LogInformation("Deleting person {PersonId}", personId);

        if (!personId.HasValue || personId == Guid.Empty)
        {
            _logger.LogWarning("Delete attempted with invalid PersonId");
            throw new ArgumentException("Invalid person Id");
        }

        var person = await _personsRepository.GetById(personId.Value);

        if (person is null)
        {
            _logger.LogWarning("Delete failed. Person {PersonId} not found", personId);
            return false;
        }

        var result = await _personsRepository.DeleteAsync(personId.Value) > 0;

        _logger.LogInformation("Person {PersonId} deleted successfully", personId);

        return result;
    }

    public async Task<List<PersonResponse>> GetAllAsync()
    {
        _logger.LogInformation("Retrieving all persons");
        return (await _personsRepository.GetAllAsync()).Select(p => p.ToPersonResponse()).ToList();
    }

    public async Task<PersonResponse?> GetByIdAsync(Guid? personId)
    {
        _logger.LogInformation("Retrieving person by Id {PersonId}", personId);

        if (!personId.HasValue)
        {
            _logger.LogWarning("GetById called with null Id");
            return null;
        }

        var person = await _personsRepository.GetById(personId.Value);

        if (person is null)
        {
            _logger.LogWarning("Person {PersonId} not found", personId);
            return null;
        }

        return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetFilteredAsync(string searchBy, string? searchValue)
    {
        _logger.LogInformation(
            "Filtering persons by {SearchBy} with value {SearchValue}",
            searchBy,
            searchValue
        );

        IEnumerable<Person>? filteredPersons = null;

        using (Operation.Time("Time for filter persons from database "))
        {
            if (string.IsNullOrWhiteSpace(searchValue))
            {
                var all = await _personsRepository.GetAllAsync();
                return all.Select(p => p.ToPersonResponse()).ToList();
            }

            searchValue = searchValue.Trim();

            var predicate = BuildExpressionFilter(searchBy, searchValue);
            filteredPersons = await _personsRepository.GetFilteredAsync(predicate);
        }

        _diagnosticContext.Set("Persons", filteredPersons);

        return filteredPersons.Select(p => p.ToPersonResponse()).ToList();
    }

    public List<PersonResponse> GetSorted(
        List<PersonResponse> persons,
        string orderBy,
        SortOrder sortOrder
    )
    {
        _logger.LogInformation(
            "Sorting persons by {OrderBy} with order {SortOrder}",
            orderBy,
            sortOrder
        );

        if (persons is null)
            return new List<PersonResponse>();

        if (string.IsNullOrEmpty(orderBy))
            return persons;

        var property = typeof(PersonResponse).GetProperty(orderBy);

        if (property == null)
        {
            _logger.LogWarning("Invalid OrderBy property {OrderBy}", orderBy);
            return persons;
        }

        object? GetKey(PersonResponse p)
        {
            var value = property.GetValue(p);

            if (value is string str)
                return str.ToLower();

            return value;
        }

        var sorted =
            sortOrder == SortOrder.DESC
                ? persons.OrderByDescending(GetKey)
                : persons.OrderBy(GetKey);

        return sorted.ToList();
    }

    public async Task<byte[]> GetPersonsCsvAsync()
    {
        _logger.LogInformation("Generating CSV export for persons");

        var memoryStream = new MemoryStream();
        CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);

        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true), leaveOpen: true))
        using (var csv = new CsvWriter(writer, configuration))
        {
            csv.WriteHeader<PersonResponse>();
            await csv.NextRecordAsync();

            var persons = await GetAllAsync();
            await csv.WriteRecordsAsync(persons);

            await writer.FlushAsync();
        }

        return memoryStream.ToArray();
    }

    public async Task<byte[]> GetPersonsExcelAsync()
    {
        _logger.LogInformation("Generating Excel export for persons");
        var persons = await GetAllAsync();

        using var excel = new ExcelPackage();
        var worksheet = excel.Workbook.Worksheets.Add("Persons");

        worksheet.Cells["A1"].Value = "Name";
        worksheet.Cells["B1"].Value = "Email";
        worksheet.Cells["C1"].Value = "Date Of Birth";
        worksheet.Cells["D1"].Value = "Age";
        worksheet.Cells["E1"].Value = "Gender";
        worksheet.Cells["F1"].Value = "Country";
        worksheet.Cells["G1"].Value = "Address";
        worksheet.Cells["H1"].Value = "Receive News Letters";

        using (var headerCells = worksheet.Cells["A1:H1"])
        {
            headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            headerCells.Style.Font.Bold = true;
        }

        int row = 2;

        foreach (var person in persons)
        {
            var age = person.DateOfBirth.HasValue
                ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25)
                    .ToString()
                : "";

            worksheet.Cells[row, 1].Value = person.Name;
            worksheet.Cells[row, 2].Value = person.Email;
            worksheet.Cells[row, 3].Value = person.DateOfBirth;
            worksheet.Cells[row, 3].Style.Numberformat.Format = "yyyy-mm-dd";
            worksheet.Cells[row, 4].Value = age;
            worksheet.Cells[row, 5].Value = person.Gender;
            worksheet.Cells[row, 6].Value = person.Country;
            worksheet.Cells[row, 7].Value = person.Address;
            worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;

            row++;
        }

        worksheet.Cells[$"A1:H{row - 1}"].AutoFitColumns();

        _logger.LogInformation("Excel export generated successfully with {RowCount} rows", row - 2);

        return await excel.GetAsByteArrayAsync();
    }

    private static Expression<Func<Person, bool>> BuildExpressionFilter(
        string searchBy,
        string searchValue
    )
    {
        searchValue = searchValue.ToLower();
        return searchBy switch
        {
            nameof(PersonResponse.Name) => p =>
                !string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(searchValue),

            nameof(PersonResponse.Email) => p =>
                !string.IsNullOrEmpty(p.Email) && p.Email.ToLower().Contains(searchValue),

            nameof(PersonResponse.Gender) => p =>
                !string.IsNullOrEmpty(p.Gender) && p.Gender.ToLower().Equals(searchValue),

            nameof(PersonResponse.Address) => p =>
                !string.IsNullOrEmpty(p.Address) && p.Address.ToLower().Contains(searchValue),

            nameof(PersonResponse.CountryId) => p =>
                p.Country != null
                && !string.IsNullOrEmpty(p.Country.Name)
                && p.Country.Name.ToLower().Contains(searchValue),

            nameof(PersonResponse.DateOfBirth) => p =>
                p.DateOfBirth.HasValue
                && p.DateOfBirth.Value.ToString("dd MMM yyyy").Contains(searchValue),

            _ => p => true,
        };
    }
}
