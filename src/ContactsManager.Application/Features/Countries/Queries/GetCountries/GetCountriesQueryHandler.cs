using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Application.Features.Countries.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

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

        if (countries.Count == 0)
        {
            return Error.NotFound("Application_Countries_NotFound", "No countries found.");
        }

        return countries.ToDtos();
    }
}
