using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Identity.Dtos;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Queries.Login;

public sealed class LoginQueryHandler(IIdentityService identityService)
    : IRequestHandler<LoginQuery, Result<AppUserDto>>
{
    private readonly IIdentityService _identityService = identityService;

    public Task<Result<AppUserDto>> Handle(LoginQuery query, CancellationToken cancellationToken)
    {
        return _identityService.LoginAsync(
            query.Email,
            query.Password,
            query.RememberMe,
            cancellationToken
        );
    }
}
