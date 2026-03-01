using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;

namespace Services;

public class PersonsService : IPersonsService
{
    private AppDbContext _context;
    private ICountriesService _countriesService;
    public PersonsService(AppDbContext context, ICountriesService countriesService)
    {
        _context = context;
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

        await _context.Persons.AddAsync(person);
        await _context.SaveChangesAsync();

        return person.ToPersonResponse();
    }



    public async Task<PersonResponse> UpdateAsync(PersonUpdateRequest? request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ValidationHelper.ValidateModel(request);

        var personToUpdate = await _context.Persons.FindAsync(request.Id);

        if (personToUpdate is null)
            throw new ArgumentException($"Person with Id {request.Id} doesn't exist");

        var country = await _countriesService.GetByIdAsync(request.CountryId);
        if (country is null)
            throw new ArgumentException("Invalid CountryId");

        personToUpdate.Name = request.Name;
        personToUpdate.Email = request.Email;
        personToUpdate.Address = request.Address;
        personToUpdate.DateOfBirth = request.DateOfBirth;
        personToUpdate.Gender = request.Gender.ToString();
        personToUpdate.CountryId = request.CountryId;
        personToUpdate.ReceiveNewsLetters = request.ReceiveNewsLetters;

        await _context.SaveChangesAsync();

        return personToUpdate.ToPersonResponse();
    }

    public async Task<bool> DeleteAsync(Guid? personId)
    {
        if (!personId.HasValue || personId == Guid.Empty)
            throw new ArgumentException("Invalid person Id");

        var person = await _context.Persons.FindAsync(personId);

        if (person is null)
            return false;

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync();

        return true;

    }

    public async Task<List<PersonResponse>> GetAllAsync()
    {
        var persons = await _context.Persons
            .AsNoTracking()
            .Include(p => p.Country)
            .ToListAsync();

        var responses = persons.Select(p => p.ToPersonResponse()).ToList();

        return responses;
    }

    public async Task<PersonResponse?> GetByIdAsync(Guid? id)
    {
        if (!id.HasValue)
            return null;

        var person = await _context.Persons
            .Include(p => p.Country)
            .FirstOrDefaultAsync(p => p.Id == id.Value);

        if (person is null)
            return null;

        return person.ToPersonResponse();
    }

    public List<PersonResponse> GetFiltered(List<PersonResponse> persons, string searchBy, string? searchValue)
    {
        if (persons is null)
            return new List<PersonResponse>();

        var personsQuery = persons.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchValue))
        {
            personsQuery = searchBy switch
            {
                nameof(PersonResponse.Name) =>
                    personsQuery.Where(p =>
                        !string.IsNullOrEmpty(p.Name) &&
                        p.Name.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),

                nameof(PersonResponse.Email) =>
                    personsQuery.Where(p =>
                        !string.IsNullOrEmpty(p.Email) &&
                        p.Email.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),

                nameof(PersonResponse.DateOfBirth) =>
                    personsQuery.Where(p =>
                        p.DateOfBirth.HasValue &&
                        p.DateOfBirth.Value.ToString("dd MMMM yyyy")
                            .Contains(searchValue, StringComparison.OrdinalIgnoreCase)),

                nameof(PersonResponse.Gender) =>
                    personsQuery.Where(p =>
                        !string.IsNullOrEmpty(p.Gender) &&
                        p.Gender.Equals(searchValue, StringComparison.OrdinalIgnoreCase)),

                nameof(PersonResponse.Country) =>
                    personsQuery.Where(p =>
                        !string.IsNullOrEmpty(p.Country) &&
                        p.Country.Contains(searchValue, StringComparison.OrdinalIgnoreCase)),

                nameof(PersonResponse.ReceiveNewsLetters) =>
                    personsQuery.Where(p =>
                        p.ReceiveNewsLetters.ToString()
                            .Equals(searchValue, StringComparison.OrdinalIgnoreCase)),

                _ => personsQuery
            };
        }

        return personsQuery.ToList();
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
        var persons = await _context.Persons
        .AsNoTracking()
        .Select(p => new
        {
            p.Name,
            p.Email,
            p.DateOfBirth,
            p.Gender,
            Country = p.Country.Name,
            p.Address,
            p.ReceiveNewsLetters
        })
        .ToListAsync();

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
}

