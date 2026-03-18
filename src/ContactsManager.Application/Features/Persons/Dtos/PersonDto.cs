using ContactsManager.Application.Features.Persons.Enums;
using ContactsManager.Domain;
using ContactsManager.Domain.Persons.Enums;

namespace ContactsManager.Application.Features.Persons.DTOs;

/// <summary>
/// DTO for returning Person details
/// </summary>
public class PersonDto : IEquatable<PersonDto>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Gender Gender { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public double? Age { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool ReceiveNewsLetters { get; set; }
    public Guid? CountryId { get; set; }
    public string? Country { get; set; }

    /// <summary>
    /// Compare current PersonResponse object with parameter
    /// </summary>
    /// <param name="other">PersonResponse Object to be compared with</param>
    /// <returns>True or False , indicating weather they are same response or not </returns>
    public bool Equals(PersonDto? other)
    {
        if (other is null)
            return false;

        return Id.Equals(other.Id)
            && Name == other.Name
            && Gender == other.Gender
            && DateOfBirth == other.DateOfBirth
            && Email == other.Email
            && Address == other.Address
            && ReceiveNewsLetters == other.ReceiveNewsLetters
            && CountryId == other.CountryId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PersonDto other)
            return false;

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Id,
            Name,
            Gender,
            Email,
            Address,
            DateOfBirth,
            ReceiveNewsLetters,
            CountryId
        );
    }

    public override string ToString()
    {
        return $"""

            ID: {Id}
            Name: {Name}
            Gender: {Gender}
            DateOfBirth: {DateOfBirth.GetValueOrDefault():dd-MMM-yyyy}
            Email: {Email}
            Address: {Address}
            Country: {Country}

            """;
    }
}
