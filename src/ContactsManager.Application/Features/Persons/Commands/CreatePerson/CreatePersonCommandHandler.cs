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
) : IRequestHandler<CreatePersonCommand, Result<Created>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<CreatePersonCommandHandler> _logger = logger;

    public async Task<Result<Created>> Handle(
        CreatePersonCommand request,
        CancellationToken cancellationToken
    )
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await _context.Persons.AnyAsync(
            p => p.Email.ToLower() == email,
            cancellationToken
        );

        if (exists)
        {
            _logger.LogWarning("Person creation aborted. Email {Email} already exists.", email);
            return Error.Conflict("Email", "A person with this email already exists");
        }

        var countryExists = await _context.Countries.AnyAsync(
            c => c.Id == request.CountryId,
            cancellationToken
        );

        if (!countryExists)
        {
            _logger.LogWarning(
                "Person creation aborted. Invalid CountryId {CountryId}.",
                request.CountryId
            );

            return Error.NotFound("CountryId", $"Country with Id {request.CountryId} not found");
        }

        var createPersonResult = Domain.Persons.Person.Create(
            Guid.NewGuid(),
            request.Name.Trim(),
            request.Gender,
            request.DateOfBirth,
            email,
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

        return Result.Created;
    }
}
