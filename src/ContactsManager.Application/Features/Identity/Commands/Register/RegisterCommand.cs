using ContactsManager.Application.Features.Identity.Dtos;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Commands.Register;

public sealed record RegisterCommand(
    string UserName,
    string Email,
    string PhoneNumber,
    string Password
) : IRequest<Result<AppUserDto>>;
