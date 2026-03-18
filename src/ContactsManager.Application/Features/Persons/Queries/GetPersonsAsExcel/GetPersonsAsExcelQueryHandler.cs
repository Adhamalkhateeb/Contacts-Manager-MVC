using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Common.Interfaces;
using ContactsManager.Application.Features.Persons.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.Queries.GetPersonsAsExcel;

public sealed class GetPersonsAsExcelQueryHandler(
    IAppDbContext context,
    ILogger<GetPersonsAsExcelQueryHandler> logger,
    IPersonExportService personExportService
) : IRequestHandler<GetPersonsAsExcelQuery, Result<byte[]>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<GetPersonsAsExcelQueryHandler> _logger = logger;
    private readonly IPersonExportService _personExportService = personExportService;

    public async Task<Result<byte[]>> Handle(
        GetPersonsAsExcelQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Generating Excel export for persons");
        var persons = await _context
            .Persons.Include(p => p.Country)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (persons is null)
        {
            _logger.LogWarning("No persons found for Excel export");
            return Error.NotFound("No persons found for Excel export");
        }

        var excelBytes = await _personExportService.GenerateExcelAsync(
            persons.ToDtos(),
            cancellationToken
        );

        if (excelBytes == null || !excelBytes.Any())
        {
            _logger.LogWarning("Failed to generate Excel export for persons");
            return Error.Failure("Failed to generate Excel export for persons");
        }

        _logger.LogInformation(
            "Excel export generated successfully with {RowCount} rows",
            persons.Count
        );

        return excelBytes;
    }
}
