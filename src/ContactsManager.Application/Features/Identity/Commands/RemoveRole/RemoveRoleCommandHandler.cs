using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Identity.Commands.RemoveRole;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Commands.RemoveRole;

public sealed class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, Result<Updated>>
{
    private readonly IIdentityService _identityService;

    public RemoveRoleCommandHandler(IIdentityService identityService) =>
        _identityService = identityService;

    public Task<Result<Updated>> Handle(
        RemoveRoleCommand command,
        CancellationToken cancellationToken
    ) => _identityService.RemoveRoleAsync(command.UserId, command.Role);
}
