using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;

public class UploadCountriesFromExcelCommandHandler(
    IAppDbContext context,
    ILogger<UploadCountriesFromExcelCommandHandler> logger,
    ICountryImportService countryImportService
) : IRequestHandler<UploadCountriesFromExcelCommand, Result<UploadCountriesFromExcelResult>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UploadCountriesFromExcelCommandHandler> _logger = logger;
    private readonly ICountryImportService _countryImportService = countryImportService;

    public async Task<Result<UploadCountriesFromExcelResult>> Handle(
        UploadCountriesFromExcelCommand request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "Starting countries Excel upload. FileName: {FileName}, Size: {FileSize}",
            request.File.FileName,
            request.File.Length
        );

        await using var fileStream = request.File.OpenReadStream();
        var namesFromExcelResult = await _countryImportService.GetCountryNamesFromExcelAsync(
            fileStream,
            cancellationToken
        );

        if (namesFromExcelResult.IsError)
        {
            _logger.LogWarning(
                "Country upload failed during Excel parsing. Errors: {Errors}",
                namesFromExcelResult.Errors.Select(e => e.Description).ToArray()
            );
            return namesFromExcelResult.TopError;
        }

        var namesFromExcel = namesFromExcelResult.Value;
        var parsedCount = namesFromExcel.Count;

        if (parsedCount == 0)
        {
            _logger.LogInformation(
                "Country upload completed with no rows to process. FileName: {FileName}",
                request.File.FileName
            );
            return new UploadCountriesFromExcelResult(0, 0, 0, 0);
        }

        var existingNames = await _context
            .Countries.AsNoTracking()
            .Select(country => country.Name)
            .ToListAsync(cancellationToken);

        var existingLookup = existingNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var insertedCount = 0;
        var duplicateCount = 0;
        var invalidCount = 0;

        foreach (var name in namesFromExcel)
        {
            if (existingLookup.Contains(name))
            {
                duplicateCount++;
                continue;
            }

            var createCountryResult = Country.Create(Guid.NewGuid(), name);
            if (createCountryResult.IsError)
            {
                invalidCount++;
                _logger.LogWarning(
                    "Skipping invalid country name during upload: {CountryName}",
                    name
                );
                continue;
            }

            _context.Countries.Add(createCountryResult.Value);
            existingLookup.Add(name);
            insertedCount++;
        }

        if (insertedCount > 0)
        {
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Country upload failed while saving changes");
                return Error.Failure(
                    "Application_UploadCountriesFromExcel_SaveFailed",
                    "Failed to save uploaded countries"
                );
            }
        }

        _logger.LogInformation(
            "Countries upload completed. ParsedRows: {ParsedRows}, Inserted: {InsertedCount}, Duplicates: {DuplicateCount}, Invalid: {InvalidCount}",
            parsedCount,
            insertedCount,
            duplicateCount,
            invalidCount
        );

        return new UploadCountriesFromExcelResult(
            parsedCount,
            insertedCount,
            duplicateCount,
            invalidCount
        );
    }
}
