using ContactsManager.Domain.Common.Results;

namespace ContactsManager.Application.Common.Interfaces;

public interface ICountryImportService
{
    Task<Result<IReadOnlyCollection<string>>> GetCountryNamesFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default
    );
}
