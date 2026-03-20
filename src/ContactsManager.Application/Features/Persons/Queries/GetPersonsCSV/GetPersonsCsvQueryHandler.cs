using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.GetPersonsCSV;

public sealed class GetPersonsCsvQueryHandler(
    ILogger<GetPersonsCsvQueryHandler> logger,
    IAppDbContext context,
    IPersonExportService personExportService
) : IRequestHandler<GetPersonsCsvQuery, Result<byte[]>>
{
    private readonly ILogger<GetPersonsCsvQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;
    private readonly IPersonExportService _personExportService = personExportService;

    public async Task<Result<byte[]>> Handle(
        GetPersonsCsvQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Generating CSV export for persons");
        var persons = await _context
            .Persons.Include(p => p.Country)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (persons is null)
        {
            _logger.LogWarning("No persons found for CSV export");
            return Error.NotFound("No persons found for CSV export");
        }

        var csvBytes = await _personExportService.GenerateCsvAsync(
            persons.ToDtos(),
            cancellationToken
        );

        if (csvBytes == null || !csvBytes.Any())
        {
            _logger.LogWarning("Failed to generate CSV export for persons");
            return Error.Failure("Failed to generate CSV export for persons");
        }
        return csvBytes;
    }
}
