using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using RepositoriesContract;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private IPersonsRepository _personsRepository;
    private ICountriesService _countriesService;

    public PersonsService(IPersonsRepository personsRepository, ICountriesService countriesService)
    {
        _personsRepository = personsRepository;
        _countriesService = countriesService;
    }


    public async Task<PersonResponse> AddAsync(PersonAddRequest? personAddRequest)
    {
        ArgumentNullException.ThrowIfNull(personAddRequest);
        ValidationHelper.ValidateModel(personAddRequest);

        var country = await _countriesService.GetByIdAsync(personAddRequest.CountryId);
        if (country is null)
            throw new ArgumentException("Invalid CountryId");

        var person = personAddRequest.ToPerson();
        person.Id = Guid.NewGuid();

        person = await _personsRepository.AddAsync(person);

        return person.ToPersonResponse();
    }

    public async Task<PersonResponse> UpdateAsync(PersonUpdateRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidationHelper.ValidateModel(request);

        var existingPerson = await _personsRepository.GetById(request.Id);

        if (existingPerson is null)
            throw new ArgumentException($"Person with Id {request.Id} doesn't exist");

        var country = await _countriesService.GetByIdAsync(request.CountryId);
        if (country is null)
            throw new ArgumentException("Invalid CountryId");


        var updatedPerson = await _personsRepository.UpdateAsync(request.ToPerson());

        return updatedPerson!.ToPersonResponse();
    }

    public async Task<bool> DeleteAsync(Guid? personId)
    {
        if (!personId.HasValue || personId == Guid.Empty)
            throw new ArgumentException("Invalid person Id");

        var person = await _personsRepository.GetById(personId.Value);

        if (person is null)
            return false;

        return await _personsRepository.DeleteAsync(personId.Value) > 0;
    }

    public async Task<List<PersonResponse>> GetAllAsync()
    {
        return (await _personsRepository.GetAllAsync())
                    .Select(p => p.ToPersonResponse())
                    .ToList();
    }

    public async Task<PersonResponse?> GetByIdAsync(Guid? personId)
    {
        if (!personId.HasValue)
            return null;

        var person = await _personsRepository.GetById(personId.Value);

        if (person is null)
            return null;

        return person.ToPersonResponse();
    }

    public async Task<List<PersonResponse>> GetFilteredAsync(string searchBy, string? searchValue)
    {

        if (string.IsNullOrWhiteSpace(searchValue))
        {
            var all = await _personsRepository.GetAllAsync();
            return all.Select(p => p.ToPersonResponse()).ToList();
        }

        searchValue = searchValue.Trim();

        var predicate = BuildExpressionFilter(searchBy, searchValue);
        var filteredPersons = await _personsRepository.GetFilteredAsync(predicate);

        return filteredPersons.Select(p => p.ToPersonResponse()).ToList();
    }

    public List<PersonResponse> GetSorted(List<PersonResponse> persons, string orderBy, SortOrder sortOrder)
    {
        if (persons is null)
            return new List<PersonResponse>();

        if (string.IsNullOrEmpty(orderBy))
            return persons;


        var property = typeof(PersonResponse).GetProperty(orderBy);
        if (property == null)
            return persons;

        object? GetKey(PersonResponse p)
        {
            var value = property.GetValue(p);

            if (value is string str)
                return str.ToLower();

            return value;
        }

        var sorted = sortOrder == SortOrder.DESC
            ? persons.OrderByDescending(GetKey)
            : persons.OrderBy(GetKey);

        return sorted.ToList();
    }

    public async Task<byte[]> GetPersonsCsvAsync()
    {
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
             ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25).ToString()
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

        return await excel.GetAsByteArrayAsync();
    }


    private static Expression<Func<Person, bool>> BuildExpressionFilter(string searchBy, string searchValue)
    {
        searchValue = searchValue.ToLower();
        return searchBy switch
        {
            nameof(PersonResponse.Name) => p =>
                !string.IsNullOrEmpty(p.Name) &&
                p.Name.ToLower().Contains(searchValue),

            nameof(PersonResponse.Email) => p =>
                !string.IsNullOrEmpty(p.Email) &&
                p.Email.ToLower().Contains(searchValue),

            nameof(PersonResponse.Gender) => p =>
                !string.IsNullOrEmpty(p.Gender) &&
                p.Gender.ToLower().Equals(searchValue),

            nameof(PersonResponse.Address) => p =>
                !string.IsNullOrEmpty(p.Address) &&
                p.Address.ToLower().Contains(searchValue),

            nameof(PersonResponse.CountryId) => p =>
                p.Country != null &&
                !string.IsNullOrEmpty(p.Country.Name) &&
                p.Country.Name.ToLower().Contains(searchValue),

            nameof(PersonResponse.DateOfBirth) => p =>
                p.DateOfBirth.HasValue &&
                p.DateOfBirth.Value.ToString("dd MMM yyyy").Contains(searchValue),

            _ => p => true
        };
    }
}

