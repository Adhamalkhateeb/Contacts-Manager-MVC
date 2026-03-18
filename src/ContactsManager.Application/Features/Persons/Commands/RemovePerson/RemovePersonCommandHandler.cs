using System;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.Commands.DeletePerson;
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
        var person = await _context.Persons.FindAsync([request.personId], cancellationToken);

        if (person == null)
        {
            _logger.LogWarning("Person with id {PersonId} was not found", request.personId);
            return Error.NotFound(
                code: "Application_RemovePerson_PersonNotFound",
                description: $"Person with id '{request.personId}' was not found"
            );
        }

        _context.Persons.Remove(person);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Person with id {PersonId} was removed successfully",
            request.personId
        );

        return Result.Deleted;
    }
}
