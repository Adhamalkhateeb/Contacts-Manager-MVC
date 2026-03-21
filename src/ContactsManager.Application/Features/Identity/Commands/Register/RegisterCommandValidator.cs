using FluentValidation;

namespace ContactsManager.Application.Features.Identity.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage("Username is required")
            .Length(3, 50)
            .WithMessage("Username must be between 3 and 50 characters")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required")
            .EmailAddress()
            .Matches(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")
            .WithMessage("Enter a valid email address");

        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .WithMessage("Phone number is required")
            .Matches(@"^\+?[1-9]\d{1,14}$")
            .WithMessage("Invalid phone number format");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters");
    }
}
