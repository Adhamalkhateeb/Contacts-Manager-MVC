using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Queries.GetFilteredPersons;

public sealed record GetFilteredPersonsQuery(string SearchBy, string? SearchValue)
    : IRequest<Result<List<PersonDto>>>;
