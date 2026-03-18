using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Queries.GetPersonById;

public sealed record class GetPersonByIdQuery(Guid personId) : IRequest<Result<PersonDto>> { }
