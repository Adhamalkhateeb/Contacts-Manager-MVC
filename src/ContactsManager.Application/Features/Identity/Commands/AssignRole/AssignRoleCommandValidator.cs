using FluentValidation;

namespace ContactsManager.Application.Features.Identity.Commands.AssignRole
{
    public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
    {
        public AssignRoleCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.Role).IsInEnum().WithMessage("Invalid role");
        }
    }
}
