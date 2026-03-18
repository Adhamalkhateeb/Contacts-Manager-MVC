using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Commands.DeletePerson;

public sealed record class RemovePersonCommand(Guid personId) : IRequest<Result<Deleted>> { }
