using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Commands.RemovePerson;

public sealed record class RemovePersonCommand(Guid PersonId) : IRequest<Result<Deleted>> { }
