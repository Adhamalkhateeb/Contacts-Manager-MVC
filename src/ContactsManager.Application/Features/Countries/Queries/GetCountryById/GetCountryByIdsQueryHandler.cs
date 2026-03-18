using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Application.Features.Countries.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Countries.Queries.GetCountryById;

public sealed class GetCountryByIdQueryHandler(
    ILogger<GetCountryByIdQueryHandler> logger,
    IAppDbContext context
) : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<GetCountryByIdQueryHandler> _logger = logger;

    public async Task<Result<CountryDto>> Handle(
        GetCountryByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        var country = await _context
            .Countries.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == query.countryId, cancellationToken);

        if (country is null)
        {
            _logger.LogWarning("Country with id {CountryId} was not found", query.countryId);

            return Error.NotFound(
                code: "Application_GetCountryById_CountryNotFound",
                description: $"Country with id '{query.countryId}' was not found"
            );
        }

        return country.ToDto();
    }
}
