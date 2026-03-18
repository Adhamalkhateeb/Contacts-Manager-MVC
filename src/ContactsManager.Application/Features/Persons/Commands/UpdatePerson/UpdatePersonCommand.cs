using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Helpers;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Persons.Enums;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Queries.UpdatePerson;

public sealed record UpdatePersonCommand(
    Guid personId,
    string Name,
    Gender Gender,
    DateTime? DateOfBirth,
    string Email,
    string? Address,
    bool ReceiveNewsLetters,
    Guid CountryId
) : IRequest<Result<Updated>>, IPersonCommand;
