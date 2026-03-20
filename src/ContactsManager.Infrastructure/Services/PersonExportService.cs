using System.Globalization;
using System.Text;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using CsvHelper;
using CsvHelper.Configuration;
using OfficeOpenXml;

namespace ContactsManager.Infrastructure.Services;

public sealed class PersonExportService : IPersonExportService
{
    public async Task<byte[]> GenerateCsvAsync(
        IReadOnlyCollection<PersonDto> persons,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var memoryStream = new MemoryStream();
        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture);

        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true), leaveOpen: true))
        using (var csv = new CsvWriter(writer, configuration))
        {
            csv.WriteHeader<PersonDto>();
            await csv.NextRecordAsync();

            await csv.WriteRecordsAsync(persons, cancellationToken);
            await writer.FlushAsync(cancellationToken);
        }

        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateExcelAsync(
        IReadOnlyCollection<PersonDto> persons,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        var row = 2;

        foreach (var person in persons)
        {
            var age = person.DateOfBirth.HasValue
                ? Math.Round((DateTime.UtcNow - person.DateOfBirth.Value).TotalDays / 365.25)
                    .ToString()
                : string.Empty;

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

        return await excel.GetAsByteArrayAsync(cancellationToken);
    }
}
