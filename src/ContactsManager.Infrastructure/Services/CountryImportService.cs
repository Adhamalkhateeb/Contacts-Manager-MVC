using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace ContactsManager.Infrastructure.Services;

public sealed class CountryImportService(ILogger<CountryImportService> logger)
    : ICountryImportService
{
    private const string ExpectedWorksheetName = "Countries";
    private const string ExpectedHeaderName = "CountryName";

    public async Task<Result<IReadOnlyCollection<string>>> GetCountryNamesFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default
    )
    {
        if (fileStream is null)
        {
            logger.LogWarning("Country Excel parsing aborted: file stream is null");
            return Error.Validation("FileRequired", "File stream is required");
        }

        if (!fileStream.CanRead)
        {
            logger.LogWarning("Country Excel parsing aborted: file stream is not readable");
            return Error.Validation("FileNotReadable", "File stream must be readable");
        }

        if (fileStream.CanSeek && fileStream.Position != 0)
            fileStream.Seek(0, SeekOrigin.Begin);

        try
        {
            using var excel = new ExcelPackage(fileStream);

            var worksheet = excel.Workbook.Worksheets[ExpectedWorksheetName];
            if (worksheet is null)
            {
                logger.LogWarning(
                    "Excel upload rejected: worksheet '{WorksheetName}' not found. Available: {Available}",
                    ExpectedWorksheetName,
                    string.Join(", ", excel.Workbook.Worksheets.Select(w => w.Name))
                );
                return Error.Validation(
                    "WorksheetNotFound",
                    $"The Excel file must contain a worksheet named '{ExpectedWorksheetName}'."
                );
            }

            if (worksheet.Dimension is null)
            {
                logger.LogInformation(
                    "Excel upload contains no data in the '{WorksheetName}' worksheet",
                    ExpectedWorksheetName
                );
                return Array.Empty<string>();
            }

            var header = worksheet.Cells[1, 1].GetValue<string>()?.Trim();
            if (!string.Equals(header, ExpectedHeaderName, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogWarning(
                    "Excel upload rejected: expected header '{ExpectedHeader}' in A1, found '{ActualHeader}'",
                    ExpectedHeaderName,
                    header
                );

                return Error.Validation(
                    "InvalidHeader",
                    $"Cell A1 must contain the column header '{ExpectedHeaderName}'. Found: '{header ?? "empty"}'."
                );
            }

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                var name = worksheet.Cells[row, 1].GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                    names.Add(name);
            }

            logger.LogInformation(
                "Parsed countries from Excel. Worksheet: {WorksheetName}, UniqueNames: {Count}",
                worksheet.Name,
                names.Count
            );

            return names.ToArray();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Country Excel parsing was canceled");
            return Error.Failure("ParsingCanceled", "Excel parsing was canceled");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse uploaded Excel file for countries");
            return Error.Validation(
                "ExcelParsingFailed",
                "Failed to parse the Excel file. Ensure it is a valid .xlsx document."
            );
        }
    }
}
