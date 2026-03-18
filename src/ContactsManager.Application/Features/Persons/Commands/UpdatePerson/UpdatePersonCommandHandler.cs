using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Persons.DTOs;
using ContactsManager.Application.Features.Persons.Mappers;
using ContactsManager.Domain.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Application.Features.Persons.Queries.UpdatePerson;

public class UpdatePersonCommandHandler(
    IAppDbContext context,
    ILogger<UpdatePersonCommandHandler> logger
) : IRequestHandler<UpdatePersonCommand, Result<Updated>>
{
    private readonly IAppDbContext _context = context;
    private readonly ILogger<UpdatePersonCommandHandler> _logger = logger;

    public async Task<Result<Updated>> Handle(
        UpdatePersonCommand request,
        CancellationToken cancellationToken
    )
    {
        var person = await _context.Persons.FindAsync(
            new object[] { request.personId },
            cancellationToken
        );

        if (person == null)
        {
            _logger.LogWarning("Person with id {PersonId} was not found", request.personId);

            return Error.NotFound(
                code: "Application_UpdatePerson_PersonNotFound",
                description: $"Person with id '{request.personId}' was not found"
            );
        }

        var exists = await _context.Persons.AnyAsync(
            p => p.Email == request.Email && p.Id != request.personId,
            cancellationToken
        );

        if (exists)
        {
            _logger.LogWarning("Person update aborted. Email already exists.");
            return Error.Conflict(
                "Application_Person_Email_Duplicate",
                "A person with this email already exists"
            );
        }

        var country = await _context.Countries.FirstOrDefaultAsync(
            c => c.Id == request.CountryId,
            cancellationToken
        );

        if (country == null)
        {
            _logger.LogWarning("Country with id {CountryId} was not found", request.CountryId);
            return Error.NotFound(
                code: "Application_UpdatePerson_CountryNotFound",
                description: $"Country with id '{request.CountryId}' was not found"
            );
        }

        var updateCustomerResult = person.Update(
            request.Name,
            request.Gender,
            request.DateOfBirth,
            request.Email,
            request.Address,
            request.ReceiveNewsLetters,
            request.CountryId
        );

        if (updateCustomerResult.IsError)
        {
            _logger.LogWarning(
                "Person update failed. Validation errors: {Errors}",
                string.Join(", ", updateCustomerResult.Errors.Select(e => e.Description))
            );

            return updateCustomerResult.Errors;
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Updated;
    }
}
