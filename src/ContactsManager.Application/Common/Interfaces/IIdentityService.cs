using ContactsManager.Application.Features.Identity.Dtos;
using ContactsManager.Domain.Common.Results;

namespace ContactsManager.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<Result<AppUserDto>> RegisterAsync(
        string userName,
        string email,
        string phoneNumber,
        string password,
        CancellationToken cancellationToken = default
    );

    Task<Result<AppUserDto>> LoginAsync(
        string email,
        string password,
        bool rememberMe = false,
        CancellationToken cancellationToken = default
    );

    Task LogoutAsync();

    Task<bool> IsEmailAvailableAsync(string email);
    Task<bool> IsUserNameAvailableAsync(string userName);
}
