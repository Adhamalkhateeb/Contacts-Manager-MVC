namespace ContactsManager.Application.Features.Countries.Commands.UploadCountryFromExcel;

public record UploadCountriesFromExcelResult(
    int ParsedCount,
    int InsertedCount,
    int DuplicateCount,
    int InvalidCount
);
