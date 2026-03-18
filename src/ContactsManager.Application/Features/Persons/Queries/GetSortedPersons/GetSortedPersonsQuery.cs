using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Enums;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Queries.GetSortedPersons;

public sealed record GetSortedPersonsQuery(
    List<PersonDto> Persons,
    string OrderBy,
    SortOrder SortOrder
) : IRequest<Result<List<PersonDto>>>;
