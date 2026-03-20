using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Enums;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Persons.Queries.GetFilteredAndSortedPersons;

/// <summary>
/// Combines filtering and sorting in a single query to reduce controller complexity
/// </summary>
public sealed record GetFilteredAndSortedPersonsQuery(
    string SearchBy,
    string? SearchValue,
    string OrderBy,
    SortOrder SortOrder
) : IRequest<Result<List<PersonDto>>>;
