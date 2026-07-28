using FluentValidation;

namespace ContactsManager.Application.Features.Identity.Commands.RemoveRole;

public sealed class RemoveRoleCommandValidator : AbstractValidator<RemoveRoleCommand>
{
    public RemoveRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Role).IsInEnum().WithMessage("Invalid role");
    }
}
