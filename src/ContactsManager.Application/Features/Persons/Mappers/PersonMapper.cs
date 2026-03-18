using System;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Domain.Persons;

namespace ContactsManager.Application.Features.Persons.Mappers;

public static class PersonMapper
{
    /// <summary>
    /// Extension method to convert object from person class to PersonDto object
    /// </summary>
    /// <param name="person">person we need to convert</param>
    /// <returns>PersonDto object fill with person data  </returns>
    public static PersonDto ToDto(this Person person)
    {
        ArgumentNullException.ThrowIfNull(person);

        return new PersonDto
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
            Age = person.DateOfBirth.HasValue
                ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25)
                : null,
        };
    }

    public static List<PersonDto> ToDtos(this IEnumerable<Person> persons)
    {
        return [.. persons.Select(p => p.ToDto())];
    }
}
