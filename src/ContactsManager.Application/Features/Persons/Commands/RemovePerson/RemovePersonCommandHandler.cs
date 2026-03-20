using System;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.Commands.RemovePerson;

public class RemovePersonCommandHandler(
    IAppDbContext context,
    ILogger<RemovePersonCommandHandler> logger
) : IRequestHandler<RemovePersonCommand, Result<Deleted>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<RemovePersonCommandHandler> _logger = logger;

    public async Task<Result<Deleted>> Handle(
        RemovePersonCommand request,
        CancellationToken cancellationToken
    )
    {
        var person = await _context.Persons.FindAsync([request.PersonId], cancellationToken);

        if (person == null)
        {
            _logger.LogWarning("Person with id {PersonId} was not found", request.PersonId);
            return Error.NotFound(
                code: "Application_RemovePerson_PersonNotFound",
                description: $"Person with id '{request.PersonId}' was not found"
            );
        }

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Person with id {PersonId} was removed successfully",
            request.PersonId
        );

        return Result.Deleted;
    }
}
