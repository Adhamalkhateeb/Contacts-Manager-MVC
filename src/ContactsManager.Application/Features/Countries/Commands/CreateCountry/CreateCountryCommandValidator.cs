using System;
using FluentValidation;

namespace ContactsManager.Application.Features.Countries.Commands.CreateCountry;

public sealed class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
        RuleFor(x => x.name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(100)
            .WithMessage("Name is too long");
    }
}
