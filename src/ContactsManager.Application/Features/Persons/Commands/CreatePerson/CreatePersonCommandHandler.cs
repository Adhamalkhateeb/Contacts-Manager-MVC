using ContactsManager.Application.Common.Errors;
using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.Commands.CreatePerson;

public sealed class CreatePersonCommandHandler(
    IAppDbContext context,
    ILogger<CreatePersonCommandHandler> logger
) : IRequestHandler<CreatePersonCommand, Result<PersonDto>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<CreatePersonCommandHandler> _logger = logger;

    public async Task<Result<PersonDto>> Handle(
        CreatePersonCommand request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.Trim().ToLower();
        var exists = await _context.Persons.AnyAsync(
            p => p.Email.ToLower() == email,
            cancellationToken
        );

        if (exists)
        {
            _logger.LogWarning("Person creation aborted. Email already exists.");
            return Error.Conflict(
                "Application_Person_Email_Duplicate",
                "A person with this email already exists"
            );
        }

        var country = await _context.Countries.FirstOrDefaultAsync(
            c => c.Id == request.CountryId,
            cancellationToken
        );

        if (country is null)
        {
            _logger.LogWarning(
                "Person creation aborted. Invalid CountryId {CountryId}.",
                request.CountryId
            );

            return Error.NotFound(
                "Application_CreatePerson_Country_NotFound",
                $"Country with Id {request.CountryId} not found"
            );
        }

        var createPersonResult = Domain.Persons.Person.Create(
            Guid.NewGuid(),
            request.Name.Trim(),
            request.Gender,
            request.DateOfBirth,
            email.Trim(),
            request.Address,
            request.ReceiveNewsLetters,
            request.CountryId
        );

        if (createPersonResult.IsError)
        {
            _logger.LogWarning("Person creation aborted. Invalid person data.");
            return createPersonResult.Errors;
        }

        var person = createPersonResult.Value;
        await _context.Persons.AddAsync(person, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return person.ToDto();
    }
}
