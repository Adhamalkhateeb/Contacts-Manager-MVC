using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Commands.AssignRole;

public sealed class AssignRoleCommandHandler(IIdentityService identityService)
    : IRequestHandler<AssignRoleCommand, Result<Updated>>
{
    private readonly IIdentityService _identityService = identityService;

    public Task<Result<Updated>> Handle(
        AssignRoleCommand command,
        CancellationToken cancellationToken
    ) => _identityService.AssignRoleAsync(command.UserId, command.Role);
}
