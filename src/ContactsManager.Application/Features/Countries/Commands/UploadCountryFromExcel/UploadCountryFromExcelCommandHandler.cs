using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;

public class UploadCountriesFromExcelCommandHandler(
    IAppDbContext context,
    ILogger<UploadCountriesFromExcelCommandHandler> logger
) : IRequestHandler<UploadCountriesFromExcelCommand, Result<int>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UploadCountriesFromExcelCommandHandler> _logger = logger;

    public async Task<Result<int>> Handle(
        UploadCountriesFromExcelCommand request,
        CancellationToken cancellationToken
    )
    {
        using var memoryStream = new MemoryStream();
        await request.file.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        using var excel = new ExcelPackage(memoryStream);
        var worksheet =
            excel.Workbook.Worksheets["Countries"] ?? excel.Workbook.Worksheets.FirstOrDefault();

        if (worksheet?.Dimension is null)
            return 0;

        var namesFromExcel = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (var row = 2; row <= worksheet.Dimension.Rows; row++)
        {
            var name = worksheet.Cells[row, 1].GetValue<string>()?.Trim();
            if (!string.IsNullOrWhiteSpace(name))
                namesFromExcel.Add(name);
        }

        if (namesFromExcel.Count == 0)
            return 0;

        var existingNames = await _context
            .Countries.AsNoTracking()
            .Select(country => country.Name)
            .ToListAsync(cancellationToken);

        var existingLookup = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var insertedCount = 0;

        foreach (var name in namesFromExcel)
        {
            if (existingLookup.Contains(name))
                continue;

            var createCountryResult = Country.Create(Guid.NewGuid(), name);
            if (createCountryResult.IsError)
            {
                _logger.LogWarning(
                    "Skipping invalid country name during upload: {CountryName}",
                    name
                );
                continue;
            }

            _context.Countries.Add(createCountryResult.Value);
            insertedCount++;
        }

        if (insertedCount > 0)
            await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Uploaded countries from Excel. Inserted: {InsertedCount}",
            insertedCount
        );

        return insertedCount;
    }
}
