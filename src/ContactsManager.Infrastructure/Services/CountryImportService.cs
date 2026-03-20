using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Common.Results;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;

namespace ContactsManager.Infrastructure.Services;

public sealed class CountryImportService(ILogger<CountryImportService> logger)
    : ICountryImportService
{
    private readonly ILogger<CountryImportService> _logger = logger;

    public async Task<Result<IReadOnlyCollection<string>>> GetCountryNamesFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default
    )
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (fileStream is null)
        {
            _logger.LogWarning("Country Excel parsing aborted: file stream is null");
            return Error.Validation(
                "Infrastructure_CountryImportService_FileStreamRequired",
                "File stream is required"
            );
        }

        if (!fileStream.CanRead)
        {
            _logger.LogWarning("Country Excel parsing aborted: file stream is not readable");
            return Error.Validation(
                "Infrastructure_CountryImportService_FileStreamNotReadable",
                "File stream must be readable"
            );
        }

        if (fileStream.CanSeek && fileStream.Position != 0)
            fileStream.Seek(0, SeekOrigin.Begin);

        try
        {
            using var excel = new ExcelPackage(fileStream);
            var worksheet =
                excel.Workbook.Worksheets["Countries"]
                ?? excel.Workbook.Worksheets.FirstOrDefault();

            if (worksheet?.Dimension is null)
            {
                _logger.LogInformation("Excel upload contains no worksheet data to parse");
                return Array.Empty<string>();
            }

            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (var row = 2; row <= worksheet.Dimension.Rows; row++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = worksheet.Cells[row, 1].GetValue<string>()?.Trim();
                if (!string.IsNullOrWhiteSpace(name))
                    names.Add(name);
            }

            _logger.LogInformation(
                "Parsed countries from Excel. Worksheet: {WorksheetName}, UniqueNames: {Count}",
                worksheet.Name,
                names.Count
            );

            return names.ToArray();
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Country Excel parsing was canceled");
            return Error.Failure(
                "Infrastructure_CountryImportService_ParsingCanceled",
                "Excel parsing was canceled"
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse uploaded Excel file for countries");
            return Error.Validation(
                "Infrastructure_CountryImportService_ExcelParsingFailed",
                "Failed to parse the Excel file. Ensure it is a valid .xlsx document with a 'Countries' worksheet and country names in the first column."
            );
        }
    }
}
