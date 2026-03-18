using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Queries;

public sealed record GetPersonsQuery : IRequest<Result<List<PersonDto>>>;
