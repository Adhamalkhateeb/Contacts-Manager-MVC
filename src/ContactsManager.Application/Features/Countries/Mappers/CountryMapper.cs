using System;
using ContactsManager.Application.Features.Countries.DTOs;
using ContactsManager.Domain.Countries;

namespace ContactsManager.Application.Features.Countries.Mappers;

public static class CountryMapper
{
    public static CountryDto ToDto(this Country country)
    {
        ArgumentNullException.ThrowIfNull(country);

        return new CountryDto { Id = country.Id, Name = country.Name };
    }

    public static List<CountryDto> ToDtos(this IEnumerable<Country> countries)
    {
        return [.. countries.Select(c => c.ToDto())];
    }
}
