using ContactsManager.Application.Features.Persons.Common;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Persons.Enums;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Commands.UpdatePerson;

public sealed record UpdatePersonCommand(
    Guid PersonId,
    string Name,
    Gender Gender,
    DateTime? DateOfBirth,
    string Email,
    string? Address,
    bool ReceiveNewsLetters,
    Guid CountryId
) : IRequest<Result<Updated>>, IPersonCommand;
