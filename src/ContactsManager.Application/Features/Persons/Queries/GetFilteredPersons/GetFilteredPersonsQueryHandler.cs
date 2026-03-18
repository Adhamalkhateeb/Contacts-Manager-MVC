using System.Linq.Expressions;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Mappers;
using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Persons;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.Queries.GetFilteredPersons;

public sealed class GetFilteredPersonsQueryHandler(
    IAppDbContext context,
    ILogger<GetFilteredPersonsQueryHandler> logger
) : IRequestHandler<GetFilteredPersonsQuery, Result<List<PersonDto>>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<GetFilteredPersonsQueryHandler> _logger = logger;

    public async Task<Result<List<PersonDto>>> Handle(
        GetFilteredPersonsQuery query,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(query.SearchValue))
        {
            var allPersons = await _context
                .Persons.Include(p => p.Country)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return allPersons.ToDtos();
        }

        var searchValue = query.SearchValue.Trim();
        var filter = BuildExpressionFilter(query.SearchBy, searchValue);
        var filteredPersons = await _context
            .Persons.Include(p => p.Country)
            .AsNoTracking()
            .Where(filter)
            .ToListAsync(cancellationToken);

        _logger.LogInformation(
            "Filtered persons query executed. SearchBy: {SearchBy}, Count: {Count}",
            query.SearchBy,
            filteredPersons.Count
        );

        return filteredPersons.ToDtos();
    }

    private static Expression<Func<Person, bool>> BuildExpressionFilter(
        string searchBy,
        string searchValue
    )
    {
        if (!string.IsNullOrEmpty(searchValue))
            searchValue = searchValue.ToLower();

        var dateParseSuccess = DateTime.TryParse(searchValue, out var parsedDate);

        return searchBy switch
        {
            nameof(PersonDto.Name) => p =>
                !string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(searchValue),

            nameof(PersonDto.Email) => p =>
                !string.IsNullOrEmpty(p.Email) && p.Email.ToLower().Contains(searchValue),

            nameof(PersonDto.Gender) => p =>
                !string.IsNullOrEmpty(p.Gender.ToString())
                && p.Gender.ToString().ToLower() == searchValue,

            nameof(PersonDto.Address) => p =>
                !string.IsNullOrEmpty(p.Address) && p.Address.ToLower().Contains(searchValue),

            nameof(PersonDto.CountryId) => p =>
                p.Country != null
                && !string.IsNullOrEmpty(p.Country.Name)
                && p.Country.Name.ToLower().Contains(searchValue),

            nameof(PersonDto.DateOfBirth) => p =>
                dateParseSuccess
                && p.DateOfBirth.HasValue
                && p.DateOfBirth.Value.Date == parsedDate.Date,

            _ => p => true,
        };
    }
}
