using System;
using System.Data.Common;
using Entities;
using ServiceContracts.Enums;

namespace ServiceContracts.DTO;

/// <summary>
/// DTO for returning Person details
/// </summary>
public class PersonResponse : IEquatable<PersonResponse>
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public string? Gender { get; set; }
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
    public bool Equals(PersonResponse? other)
    {
        if (other is null)
            return false;

        return Id.Equals(other.Id) &&
                Name == other.Name &&
                Gender == other.Gender &&
                DateOfBirth == other.DateOfBirth &&
                Email == other.Email &&
                Address == other.Address &&
                ReceiveNewsLetters == other.ReceiveNewsLetters &&
                CountryId == other.CountryId;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PersonResponse other)
            return false;

        return Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Id, Name, Gender, Email, Address,
            DateOfBirth, ReceiveNewsLetters, CountryId
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

    public PersonUpdateRequest ToPersonUpdateRequest()
    {
        return new PersonUpdateRequest
        {
            Id = Id,
            Name = Name,
            Gender = Enum.Parse<Gender>(Gender!, true),
            DateOfBirth = DateOfBirth,
            Email = Email,
            Address = Address,
            ReceiveNewsLetters = ReceiveNewsLetters,
            CountryId = CountryId!.Value

        };
    }
}

public static class PersonExtensions
{

    /// <summary>
    /// Extension method to convert object from person class to PersonResponse object
    /// </summary>
    /// <param name="person">person we need to convert</param>
    /// <returns>PersonResponse object fill with person data  </returns>
    public static PersonResponse ToPersonResponse(this Person person)
    {
        return new PersonResponse
        {
            Id = person.Id,
            Name = person.Name,
            Gender = person.Gender,
            DateOfBirth = person.DateOfBirth,
            Email = person.Email,
            Address = person.Address,
            ReceiveNewsLetters = person.ReceiveNewsLetters,
            CountryId = person.CountryId,
            Country = person.Country?.Name,
            Age = (person.DateOfBirth != null) ?
            Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25) : null
        };
    }
}
