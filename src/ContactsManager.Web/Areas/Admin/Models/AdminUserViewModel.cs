namespace ContactsManager.Web.Areas.Admin.Models;

public sealed record AdminUserViewModel(
    Guid Id,
    string UserName,
    string Email,
    IReadOnlyCollection<string> Roles,
    bool IsAdmin
);
