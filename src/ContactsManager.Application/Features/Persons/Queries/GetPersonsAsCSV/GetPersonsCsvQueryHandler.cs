using System.Globalization;
using System.Text;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Domain.Common.Results;
using CsvHelper;
using CsvHelper.Configuration;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.GetPersonsCSV;

public sealed class GetPersonsCsvQueryHandler(
    ILogger<GetPersonsCsvQueryHandler> logger,
    IAppDbContext context
) : IRequestHandler<GetPersonsCsvQuery, Result<byte[]>>
{
    private readonly ILogger<GetPersonsCsvQueryHandler> _logger = logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<byte[]>> Handle(
        GetPersonsCsvQuery request,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation("Generating CSV export for persons");

        var memoryStream = new MemoryStream();
        CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture);

        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(true), leaveOpen: true))
        using (var csv = new CsvWriter(writer, configuration))
        {
            csv.WriteHeader<PersonDto>();
            await csv.NextRecordAsync();

            var persons = await _context.Persons.AsNoTracking().ToListAsync();
            await csv.WriteRecordsAsync(persons);

            await writer.FlushAsync();
        }

        return memoryStream.ToArray();
    }
}
