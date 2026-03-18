using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Enums;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.Queries.GetSortedPersons;

public sealed class GetSortedPersonsQueryHandler(ILogger<GetSortedPersonsQueryHandler> logger)
    : IRequestHandler<GetSortedPersonsQuery, Result<List<PersonDto>>>
{
    private readonly ILogger<GetSortedPersonsQueryHandler> _logger = logger;

    public async Task<Result<List<PersonDto>>> Handle(
        GetSortedPersonsQuery query,
        CancellationToken cancellationToken
    )
    {
        _logger.LogInformation(
            "Sorting {Count} persons by {OrderBy} in {SortOrder} order",
            query.Persons.Count,
            query.OrderBy,
            query.SortOrder
        );
        if (query.Persons.Count == 0 || string.IsNullOrWhiteSpace(query.OrderBy))
            return query.Persons;

        var property = typeof(PersonDto).GetProperty(query.OrderBy);
        if (property is null)
            return query.Persons;

        object? GetKey(PersonDto person)
        {
            var value = property.GetValue(person);
            return value is string str ? str.ToLowerInvariant() : value;
        }

        var sorted =
            query.SortOrder == SortOrder.DESC
                ? query.Persons.OrderByDescending(GetKey).ToList()
                : query.Persons.OrderBy(GetKey).ToList();

        _logger.LogInformation("Sorting completed.");

        return sorted;
    }
}
