namespace ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;

public sealed record UploadCountriesFromExcelResult(
    int ParsedCount,
    int InsertedCount,
    int DuplicateCount,
    int InvalidCount
);
