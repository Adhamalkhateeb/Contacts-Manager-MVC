using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Identity.Dtos;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersQuery, Result<List<AppUserDto>>>
{
    private readonly IIdentityService _identityService = identityService;

    public Task<Result<List<AppUserDto>>> Handle(
        GetUsersQuery request,
        CancellationToken cancellationToken
    ) => _identityService.GetUsersAsync(cancellationToken);
}
