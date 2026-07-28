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
    public async Task<Result<UploadCountriesFromExcelResult>> Handle(
        UploadCountriesFromExcelCommand request,
        CancellationToken cancellationToken
    )
    {
        logger.LogInformation(
            "Starting countries Excel upload. FileName: {FileName}, Size: {FileSize}",
            request.FileName,
            request.FileSize
        );

        var namesFromExcelResult = await countryImportService.GetCountryNamesFromExcelAsync(
            request.FileStream,
            cancellationToken
        );

        if (namesFromExcelResult.IsError)
        {
            logger.LogWarning(
                "Country upload failed during Excel parsing. Errors: {Errors}",
                string.Join(", ", namesFromExcelResult.Errors.Select(e => e.Description))
            );
            return namesFromExcelResult.TopError;
        }

        var namesFromExcel = namesFromExcelResult.Value;
        var parsedCount = namesFromExcel.Count;

        if (parsedCount == 0)
        {
            logger.LogInformation(
                "Country upload completed with no rows to process. FileName: {FileName}",
                request.FileName
            );
            return new UploadCountriesFromExcelResult(0, 0, 0, 0);
        }

        var existingNames = await context
            .Countries.AsNoTracking()
            .Select(c => c.Name)
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

            var createResult = Country.Create(Guid.NewGuid(), name);
            if (createResult.IsError)
            {
                invalidCount++;
                logger.LogWarning("Skipping invalid country name: {CountryName}", name);
                continue;
            }

            context.Countries.Add(createResult.Value);
            existingLookup.Add(name);
            insertedCount++;
        }

        if (insertedCount > 0)
        {
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException ex)
            {
                logger.LogError(ex, "Country upload failed while saving changes");
                return Error.Failure(
                    "Application_UploadCountriesFromExcel_SaveFailed",
                    "Failed to save uploaded countries"
                );
            }
        }

        logger.LogInformation(
            "Countries upload completed. Parsed: {Parsed}, Inserted: {Inserted}, Duplicates: {Duplicates}, Invalid: {Invalid}",
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
