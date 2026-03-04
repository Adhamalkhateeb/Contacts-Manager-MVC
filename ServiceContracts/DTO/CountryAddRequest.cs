using System;
using Entities;

namespace ServiceContracts.DTO;

/// <summary>
/// DTO Class for adding new Country
/// </summary>
public class CountryAddRequest
{
    public string? Name { get; set; }

    public Country ToCountry()
    {
        return new Country { Name = Name };
    }
}
