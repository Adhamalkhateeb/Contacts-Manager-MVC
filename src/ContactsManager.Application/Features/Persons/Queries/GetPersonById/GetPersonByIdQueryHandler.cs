using System;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.Queries.GetPersonById;

public class GetPersonByIdQueryHandler(
    IAppDbContext context,
    ILogger<GetPersonByIdQueryHandler> logger
) : IRequestHandler<GetPersonByIdQuery, Result<PersonDto>>
{
    private readonly IAppDbContext _context = context;

    public async Task<Result<PersonDto>> Handle(
        GetPersonByIdQuery query,
        CancellationToken cancellationToken
    )
    {
        var person = await _context.Persons.FindAsync([query.personId], cancellationToken);
        if (person is null)
        {
            logger.LogWarning("Person with id {Id} not found", query.personId);
            return Error.NotFound(
                code: "Application_GetPersonById_PersonNotFound",
                description: $"Person with id '{query.personId}' was not found"
            );
        }

        return person.ToDto();
    }
}
