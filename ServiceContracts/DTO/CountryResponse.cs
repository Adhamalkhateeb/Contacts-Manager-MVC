using System;
using Entities;

namespace ServiceContracts.DTO;
/// <summary>
/// Return Type for most CountryService Methods
/// </summary>
public class CountryResponse : IEquatable<CountryResponse>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool Equals(CountryResponse? other)
    {
        if (other is null)
            return false;
        return Id.Equals(other.Id) && Name == other.Name;
    }


    public override bool Equals(object? obj)
    {
        if (obj is not CountryResponse other)
            return false;

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Name);
    }

    public override string ToString()
    {
        return $"""

            Country Id  : {Id}
            Country Name: {Name}
            
        """;
    }
}

public static class CountryExtensions
{
    public static CountryResponse ToCountryResponse(this Country country)
    {
        return new CountryResponse
        {
            Id = country.Id,
            Name = country.Name
        };
    }
}
