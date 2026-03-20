using System;
using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Commands.RemovePerson;

public sealed class RemovePersonCommandValidator : AbstractValidator<RemovePersonCommand>
{
    public RemovePersonCommandValidator()
    {
        RuleFor(command => command.PersonId)
            .NotEmpty()
            .WithMessage("PersonId is required.")
            .Must(id => id != Guid.Empty)
            .WithMessage("PersonId cannot be an empty GUID.");
    }
}
