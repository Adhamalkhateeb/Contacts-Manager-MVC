namespace ContactsManager.Application.Features.Identity.Dtos;

public sealed record AppUserDto(
    Guid UserId,
    string UserName,
    string Email,
    string PhoneNumber,
    IList<string> Roles
);
