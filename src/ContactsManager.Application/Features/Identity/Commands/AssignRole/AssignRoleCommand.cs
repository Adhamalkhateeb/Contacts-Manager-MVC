using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Identity;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Commands.AssignRole
{
	public sealed record AssignRoleCommand(Guid UserId, Role Role) : IRequest<Result<Updated>>;
}
