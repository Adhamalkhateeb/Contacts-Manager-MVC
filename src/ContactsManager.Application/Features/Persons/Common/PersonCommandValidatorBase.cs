using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Helpers;

public abstract class PersonCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IPersonCommand
{
    protected void AddCommonRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("person name is too long,maximum 100 character.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(255)
            .WithMessage("Email is too long,maximum 255 characters.")
            .EmailAddress()
            .WithMessage("Email is in invalid format.");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("maximum character is 500")
            .When(x => x.Address is not null);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow)
            .WithMessage("")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.CountryId).NotEmpty().WithMessage("CountryId is required");
    }
}
