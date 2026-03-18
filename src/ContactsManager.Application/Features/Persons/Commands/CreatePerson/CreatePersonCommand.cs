using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Helpers;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Persons.Enums;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Commands.CreatePerson;

public sealed record CreatePersonCommand(
    string Name,
    Gender Gender,
    DateTime? DateOfBirth,
    string Email,
    string? Address,
    bool ReceiveNewsLetters,
    Guid CountryId
) : IRequest<Result<PersonDto>>, IPersonCommand;
