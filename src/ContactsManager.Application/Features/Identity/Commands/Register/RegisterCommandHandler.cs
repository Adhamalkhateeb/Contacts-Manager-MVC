using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Identity.Dtos;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Commands.Register;

public sealed class RegisterCommandHandler(IIdentityService identityService)
    : IRequestHandler<RegisterCommand, Result<AppUserDto>>
{
    private readonly IIdentityService _identityService = identityService;

    public Task<Result<AppUserDto>> Handle(
        RegisterCommand request,
        CancellationToken cancellationToken
    )
    {
        var result = _identityService.RegisterAsync(
            userName: request.UserName,
            email: request.Email,
            phoneNumber: request.PhoneNumber,
            password: request.Password,
            cancellationToken: cancellationToken
        );

        return result;
    }
}
