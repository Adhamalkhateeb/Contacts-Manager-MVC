using ContactsManager.Domain.Common.Results;

namespace ContactsManager.Application.Features.Common.Interfaces;

public interface ICountryImportService
{
    Task<Result<IReadOnlyCollection<string>>> GetCountryNamesFromExcelAsync(
        Stream fileStream,
        CancellationToken cancellationToken = default
    );
}
