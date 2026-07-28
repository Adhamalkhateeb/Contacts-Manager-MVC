using ContactsManager.Domain.Common.Results;
using ContactsManager.Domain.Identity;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Commands.RemoveRole
{
    public sealed record RemoveRoleCommand(Guid UserId, Role Role) : IRequest<Result<Updated>>;
}
