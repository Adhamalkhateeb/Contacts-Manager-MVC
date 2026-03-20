using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Application.Features.Countries.Mappers;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Countries;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Countries.Commands.CreateCountry;

public sealed class CreateCountryCommandHandler(
    IAppDbContext context,
    ILogger<CreateCountryCommandHandler> Logger
) : IRequestHandler<CreateCountryCommand, Result<CountryDto>>
{
    private readonly ILogger<CreateCountryCommandHandler> _logger = Logger;
    private readonly IAppDbContext _context = context;

    public async Task<Result<CountryDto>> Handle(
        CreateCountryCommand request,
        CancellationToken cancellationToken
    )
    {
        var countryName = request.name.Trim().ToLower();
        var exists = await _context.Countries.AnyAsync(
            c => c.Name.ToLower() == countryName,
            cancellationToken
        );

        if (exists)
        {
            _logger.LogWarning("Country creation aborted. country already exists.");
            return Error.Conflict(
                "Application_CreateCountry_CountryExists",
                $"A country with the name '{request.name}' already exists"
            );
        }

        var createCountryResult = Country.Create(Guid.NewGuid(), countryName);
        if (createCountryResult.IsError)
        {
            return createCountryResult.Errors;
        }

        _context.Countries.Add(createCountryResult.Value);
        await _context.SaveChangesAsync(cancellationToken);

        var customer = createCountryResult.Value;

        _logger.LogInformation(
            "Country created successfully. Id: {CustomerId}",
            createCountryResult.Value.Id
        );

        return customer.ToDto();
    }
}
