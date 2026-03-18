using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace ContactsManager.Application.Features.Persons.Queries.GetPersonsAsExcel;

public sealed class GetPersonsAsExcelQueryHandler(
    IAppDbContext context,
    ILogger<GetPersonsAsExcelQueryHandler> logger
) : IRequestHandler<GetPersonsAsExcelQuery, Result<byte[]>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<GetPersonsAsExcelQueryHandler> _logger = logger;

    public async Task<Result<byte[]>> Handle(
        GetPersonsAsExcelQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Generating Excel export for persons");
        var persons = await _context.Persons.AsNoTracking().ToListAsync();

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
}
