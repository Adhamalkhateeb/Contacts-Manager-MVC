using ContactsManager.Application.Common.Interfaces;
using ContactsManager.Application.Features.Identity.Dtos;
using ContactsManager.Domain.Common.Results;
using Microsoft.AspNetCore.Identity;

namespace ContactsManager.Infrastructure.Identity;

public class IdentityService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
    : IIdentityService
{
    private readonly UserManager<AppUser> _userManager = userManager;
    private readonly SignInManager<AppUser> _signInManager = signInManager;

    public async Task<Result<AppUserDto>> LoginAsync(
        string email,
        string password,
        bool rememberMe = false,
        CancellationToken cancellationToken = default
    )
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null)
            return Error.Unauthorized(
                code: "Identity.InvalidCredentials",
                description: "Invalid email or password."
            );

        var signInResult = await _signInManager.PasswordSignInAsync(
            user,
            password,
            isPersistent: rememberMe,
            lockoutOnFailure: true
        );

        if (signInResult.IsLockedOut)
            return Error.Forbidden(
                code: "Identity.LockedOut",
                description: "Account is locked. Try again later."
            );

        if (!signInResult.Succeeded)
            return Error.Unauthorized(
                code: "Identity.InvalidCredentials",
                description: "Invalid email or password."
            );

        return new AppUserDto(
            UserId: user.Id,
            UserName: user.UserName!,
            Email: user.Email!,
            PhoneNumber: user.PhoneNumber!,
            Roles: new List<string>()
        );
    }

    public async Task<Result<AppUserDto>> RegisterAsync(
        string userName,
        string email,
        string phoneNumber,
        string password,
        CancellationToken cancellationToken = default
    )
    {
        if (await _userManager.FindByEmailAsync(email) is not null)
            return Error.Conflict("Email", "A user with this email already exists.");

        if (await _userManager.FindByNameAsync(userName) is not null)
            return Error.Conflict("Username", "This username is already taken.");

        var user = new AppUser
        {
            UserName = userName,
            Email = email,
            PhoneNumber = phoneNumber,
        };

        var createResult = await _userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
            return MapIdentityErrors(createResult.Errors);

        return new AppUserDto(
            UserId: user.Id,
            UserName: user.UserName!,
            Email: user.Email!,
            PhoneNumber: user.PhoneNumber!,
            Roles: new List<string>()
        );
    }

    private static List<Error> MapIdentityErrors(IEnumerable<IdentityError> identityErrors) =>
        identityErrors
            .Select(e => Error.Conflict(code: e.Code, description: e.Description))
            .ToList();

    public async Task LogoutAsync() => await _signInManager.SignOutAsync();

    public async Task<bool> IsEmailAvailableAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is null;
    }

    public async Task<bool> IsUserNameAvailableAsync(string userName)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user is null;
    }
}
