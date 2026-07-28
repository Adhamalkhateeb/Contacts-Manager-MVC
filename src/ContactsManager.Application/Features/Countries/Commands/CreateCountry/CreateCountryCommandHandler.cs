using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Application.Features.Countries.Mappers;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Countries.Commands.CreateCountry;

public class CreateCountryCommandHandler(
    IAppDbContext dbContext,
    ILogger<CreateCountryCommandHandler> logger
) : IRequestHandler<CreateCountryCommand, Result<CountryDto>>
{
    private readonly IAppDbContext _context = dbContext;
    private readonly ILogger<CreateCountryCommandHandler> _logger = logger;

    public async Task<Result<CountryDto>> Handle(
        CreateCountryCommand request,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return CountryErrors.NameRequired;
        }

        var formattedName = request.Name.Trim().ToLowerInvariant();

        var exists = await _context.Countries.AnyAsync(
            c => c.Name.ToLower() == formattedName,
            cancellationToken
        );

        if (exists)
        {
            return Error.Conflict(
                "Application_CreateCountry_CountryExists",
                "Country already exists."
            );
        }

        var countryResult = Country.Create(Guid.NewGuid(), formattedName);
        if (countryResult.IsError)
        {
            return countryResult.Errors;
        }

        _context.Countries.Add(countryResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Country {CountryName} created successfully.", formattedName);

        return countryResult.Value.ToDto();
    }
}
