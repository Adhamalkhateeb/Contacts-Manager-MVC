using ContactsManager.Application.Features.Identity.Dtos;
using ContactsManager.Domain.Common.Results;
using MediatR;

namespace ContactsManager.Application.Features.Identity.Queries.Login;

public sealed record LoginQuery(string Email, string Password, bool RememberMe = false)
    : IRequest<Result<AppUserDto>>;
