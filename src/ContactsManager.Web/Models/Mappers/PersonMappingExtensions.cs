using ContactsManager.Application.Features.Persons.Commands.CreatePerson;
using ContactsManager.Application.Features.Persons.Commands.UpdatePerson;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Contracts.Requests.Person;
using ContactsManager.Contracts.Responses;
using ContactsManager.Domain.Persons.Enums;

namespace ContactsManager.Web.Models.Mappers;

/// <summary>
/// Extension methods for mapping between DTOs and Response/Request models
/// </summary>
public static class PersonMappingExtensions
{
    /// <summary>
    /// Map PersonDto to PersonResponse
    /// </summary>
    public static PersonResponse ToPersonResponse(this PersonDto dto)
    {
        return new PersonResponse
        {
            Id = dto.Id,
            Name = dto.Name,
            Gender = dto.Gender.ToString(),
            DateOfBirth = dto.DateOfBirth,
            Age = dto.Age,
            Email = dto.Email,
            Address = dto.Address,
            ReceiveNewsLetters = dto.ReceiveNewsLetters,
            CountryId = dto.CountryId,
            Country = dto.Country,
        };
    }

    /// <summary>
    /// Map collection of PersonDto to PersonResponse collection
    /// </summary>
    public static IEnumerable<PersonResponse> ToPersonResponses(this IEnumerable<PersonDto> dtos)
    {
        return [.. dtos.Select(dto => dto.ToPersonResponse())];
    }

    /// <summary>
    /// Map CreatePersonRequest to CreatePersonCommand
    /// </summary>
    public static CreatePersonCommand ToCreatePersonCommand(this CreatePersonRequest request)
    {
        return new CreatePersonCommand(
            request.Name!,
            (Gender)request.Gender!,
            request.DateOfBirth,
            request.Email!,
            request.Address,
            request.ReceiveNewsLetters,
            request.CountryId!.Value
        );
    }

    /// <summary>
    /// Map UpdatePersonRequest to UpdatePersonCommand parameters
    /// </summary>
    public static UpdatePersonCommand ToUpdatePersonCommand(this UpdatePersonRequest request)
    {
        return new(
            request.Id,
            request.Name!,
            (Gender)request.Gender!,
            request.DateOfBirth,
            request.Email!,
            request.Address,
            request.ReceiveNewsLetters,
            request.CountryId
        );
    }

    public static UpdatePersonRequest ToUpdatePersonRequest(this PersonDto dto)
    {
        return new UpdatePersonRequest
        {
            Id = dto.Id,
            Name = dto.Name,
            Gender = (Contracts.Requests.Person.Enums.Gender)dto.Gender,
            DateOfBirth = dto.DateOfBirth,
            Email = dto.Email,
            Address = dto.Address,
            ReceiveNewsLetters = dto.ReceiveNewsLetters,
            CountryId = dto.CountryId!.Value,
        };
    }
}
