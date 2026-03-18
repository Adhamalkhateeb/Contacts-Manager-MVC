using ContactsManager.Domain.Countries;

namespace ContactsManager.Application.Features.Countries.DTOs;

/// <summary>
/// Represent Data Transfer object between layers
/// </summary>
public class CountryDto : IEquatable<CountryDto>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }

    public bool Equals(CountryDto? other)
    {
        if (other is null)
            return false;
        return Id.Equals(other.Id) && Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CountryDto other)
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
