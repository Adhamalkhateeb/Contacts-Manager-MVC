using System;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ContactsManager.Application.Features.Persons.Queries.GetPersons;

public sealed class GetPersonsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPersonsQuery, Result<List<PersonDto>>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<List<PersonDto>>> Handle(
        GetPersonsQuery request,
        CancellationToken cancellationToken
    )
    {
        var persons = await _context.Persons.AsNoTracking().ToListAsync();
        return persons.ToDtos();
    }
}
