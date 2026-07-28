using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Application.Features.Countries.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Countries.Queries.GetCountryById;

public class GetCountryByIdQueryHandler(
    ILogger<GetCountryByIdQueryHandler> logger,
    IAppDbContext dbContext
) : IRequestHandler<GetCountryByIdQuery, Result<CountryDto>>
{
    private readonly ILogger<GetCountryByIdQueryHandler> _logger = logger;
    private readonly IAppDbContext _dbContext = dbContext;

    public async Task<Result<CountryDto>> Handle(
        GetCountryByIdQuery request,
        CancellationToken cancellationToken
    )
    {
        var country = await _dbContext
            .Countries.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken);

        if (country is null)
        {
            _logger.LogWarning("Country with Id {CountryId} not found.", request.Id);
            return Error.NotFound(
                "Application_GetCountryById_CountryNotFound",
                "Country not found."
            );
        }

        return country.ToDto();
    }
}
