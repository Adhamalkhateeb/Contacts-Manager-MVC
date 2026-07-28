using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Enums;
using ContactsManager.Application.Features.Persons.Mappers;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Persons;
using ContactsManager.Domain.Persons.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.Queries.GetFilteredAndSortedPersons;

/// <summary>
/// Handles both filtering and sorting of persons in a single query
/// </summary>
public sealed class GetFilteredAndSortedPersonsQueryHandler(
    IAppDbContext context,
    ILogger<GetFilteredAndSortedPersonsQueryHandler> logger
) : IRequestHandler<GetFilteredAndSortedPersonsQuery, Result<List<PersonDto>>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<GetFilteredAndSortedPersonsQueryHandler> _logger = logger;

    public async Task<Result<List<PersonDto>>> Handle(
        GetFilteredAndSortedPersonsQuery query,
        CancellationToken cancellationToken
    )
    {
        var personsQuery = _context.Persons.Include(p => p.Country).AsNoTracking();

        personsQuery = ApplyFilter(personsQuery, query.SearchBy, query.SearchValue);
        personsQuery = ApplySort(personsQuery, query.OrderBy, query.SortOrder);

        var filteredPersons = await personsQuery.ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Filtered persons query executed. SearchBy: {SearchBy}, Count: {Count}",
            query.SearchBy,
            filteredPersons.Count
        );

        _logger.LogInformation(
            "Sorting completed. OrderBy: {OrderBy}, SortOrder: {SortOrder}",
            query.OrderBy,
            query.SortOrder
        );

        return filteredPersons.ToDtos();
    }

    private static IQueryable<Person> ApplyFilter(
        IQueryable<Person> query,
        string searchBy,
        string? searchValue
    )
    {
        if (string.IsNullOrWhiteSpace(searchValue))
            return query;

        var search = searchValue.Trim();
        var loweredSearch = search.ToLower();
        var dateParseSuccess = DateTime.TryParse(search, out var parsedDate);

        return searchBy switch
        {
            nameof(PersonDto.Name) => query.Where(p =>
                !string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(loweredSearch)
            ),

            nameof(PersonDto.Email) => query.Where(p =>
                !string.IsNullOrEmpty(p.Email) && p.Email.ToLower().Contains(loweredSearch)
            ),

            nameof(PersonDto.Gender) when Enum.TryParse<Gender>(search, true, out var gender) =>
                query.Where(p => p.Gender == gender),

            nameof(PersonDto.Gender) => query.Where(_ => false),

            nameof(PersonDto.Address) => query.Where(p =>
                !string.IsNullOrEmpty(p.Address) && p.Address.ToLower().Contains(loweredSearch)
            ),

            nameof(PersonDto.CountryId) => query.Where(p =>
                p.Country != null
                && !string.IsNullOrEmpty(p.Country.Name)
                && p.Country.Name.ToLower().Contains(loweredSearch)
            ),

            nameof(PersonDto.DateOfBirth) when dateParseSuccess => query.Where(p =>
                p.DateOfBirth.HasValue && p.DateOfBirth.Value.Date == parsedDate.Date
            ),

            nameof(PersonDto.DateOfBirth) => query.Where(_ => false),

            _ => query,
        };
    }

    private static IQueryable<Person> ApplySort(
        IQueryable<Person> query,
        string orderBy,
        SortOrder sortOrder
    )
    {
        if (string.IsNullOrWhiteSpace(orderBy))
            return query;

        return orderBy switch
        {
            nameof(PersonDto.Name) => sortOrder == SortOrder.DESC
                ? query.OrderByDescending(p => p.Name)
                : query.OrderBy(p => p.Name),

            nameof(PersonDto.Email) => sortOrder == SortOrder.DESC
                ? query.OrderByDescending(p => p.Email)
                : query.OrderBy(p => p.Email),

            nameof(PersonDto.DateOfBirth) => sortOrder == SortOrder.DESC
                ? query.OrderByDescending(p => p.DateOfBirth)
                : query.OrderBy(p => p.DateOfBirth),

            nameof(PersonDto.Age) => sortOrder == SortOrder.DESC
                ? query.OrderByDescending(p => p.DateOfBirth)
                : query.OrderBy(p => p.DateOfBirth),

            nameof(PersonDto.Gender) => sortOrder == SortOrder.DESC
                ? query.OrderByDescending(p => p.Gender)
                : query.OrderBy(p => p.Gender),

            nameof(PersonDto.Address) => sortOrder == SortOrder.DESC
                ? query.OrderByDescending(p => p.Address)
                : query.OrderBy(p => p.Address),

            nameof(PersonDto.CountryId) => sortOrder == SortOrder.DESC
                ? query.OrderByDescending(p => p.Country.Name)
                : query.OrderBy(p => p.Country.Name),

            nameof(PersonDto.ReceiveNewsLetters) => sortOrder == SortOrder.DESC
                ? query.OrderByDescending(p => p.ReceiveNewsLetters)
                : query.OrderBy(p => p.ReceiveNewsLetters),

            _ => query,
        };
    }
}
