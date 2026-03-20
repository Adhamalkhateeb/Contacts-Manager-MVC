using ContactsManager.Application.Features.Persons.DTOs;

namespace ContactsManager.Application.Common.Interfaces;

public interface IPersonExportService
{
    Task<byte[]> GenerateCsvAsync(
        IReadOnlyCollection<PersonDto> persons,
        CancellationToken cancellationToken = default
    );

    Task<byte[]> GenerateExcelAsync(
        IReadOnlyCollection<PersonDto> persons,
        CancellationToken cancellationToken = default
    );
}
