using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Application.Features.Countries.Mappers;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Countries.Queries.GetCountries;

public class GetCountriesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetCountriesQuery, Result<List<CountryDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<List<CountryDto>>> Handle(
        GetCountriesQuery request,
        CancellationToken cancellationToken
    )
    {
        var countries = await _context.Countries.AsNoTracking().ToListAsync(cancellationToken);

        if (!countries.Any())
            return Error.NotFound(
                "Application_Countries_NotFound",
                "No countries found. Please add some countries first."
            );

        return countries.ToDtos();
    }
}
