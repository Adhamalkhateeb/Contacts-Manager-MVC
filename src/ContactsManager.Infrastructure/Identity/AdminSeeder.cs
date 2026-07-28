using ContactsManager.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ContactsManager.Infrastructure.Identity;

/// <summary>
/// Seeds a default admin account on first startup.
/// The password should be changed after first login.
/// </summary>
public static class AdminSeeder
{
    private const string AdminEmail = "admin@contactsmanager.com";
    private const string AdminUserName = "admin";
    private const string AdminPassword = "Admin@1234";
    private const string AdminPhone = "+201021094971";

    public static async Task SeedAsync(UserManager<AppUser> userManager, ILogger logger)
    {
        var existing = await userManager.FindByEmailAsync(AdminEmail);

        if (existing is not null)
        {
            if (!await userManager.IsInRoleAsync(existing, nameof(Role.Admin)))
            {
                var addResult = await userManager.AddToRoleAsync(existing, nameof(Role.Admin));
                if (!addResult.Succeeded)
                    logger.LogError(
                        "AdminSeeder: failed to assign Admin role to existing user. Errors: {Errors}",
                        string.Join(", ", addResult.Errors.Select(e => e.Description))
                    );
                else
                    logger.LogInformation(
                        "AdminSeeder: Admin role assigned to existing user '{Email}'.",
                        AdminEmail
                    );
            }

            return;
        }

        var user = new AppUser
        {
            UserName = AdminUserName,
            Email = AdminEmail,
            PhoneNumber = AdminPhone,
        };

        var createResult = await userManager.CreateAsync(user, AdminPassword);
        if (!createResult.Succeeded)
        {
            logger.LogError(
                "AdminSeeder: failed to create admin account. Errors: {Errors}",
                string.Join(", ", createResult.Errors.Select(e => e.Description))
            );
            return;
        }

        var roleResult = await userManager.AddToRoleAsync(user, nameof(Role.Admin));
        if (!roleResult.Succeeded)
        {
            logger.LogError(
                "AdminSeeder: account created but Admin role assignment failed. Errors: {Errors}",
                string.Join(", ", roleResult.Errors.Select(e => e.Description))
            );
            return;
        }

        logger.LogInformation(
            "AdminSeeder: default admin account '{Email}' created successfully. "
                + "Change the default password after first login.",
            AdminEmail
        );
    }
}
