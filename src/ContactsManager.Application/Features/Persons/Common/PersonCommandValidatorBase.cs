using FluentValidation;

namespace ContactsManager.Application.Features.Persons.Common;

public abstract class PersonCommandValidatorBase<TCommand> : AbstractValidator<TCommand>
    where TCommand : IPersonCommand
{
    protected void AddCommonRules()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(100)
            .WithMessage("person name is too long , maximum 100 character.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(255)
            .WithMessage("Email is too long,maximum 255 characters.")
            .EmailAddress()
            .Matches(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        RuleFor(x => x.Address)
            .MaximumLength(500)
            .WithMessage("Address maximum character is 500")
            .When(x => x.Address is not null);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow)
            .WithMessage("Date of birth can't be in the future.")
            .When(x => x.DateOfBirth.HasValue);

        RuleFor(x => x.CountryId).NotEmpty().WithMessage("CountryId is required");
    }
}
