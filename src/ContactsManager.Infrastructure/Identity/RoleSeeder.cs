using ContactsManager.Domain.Identity;
using Microsoft.AspNetCore.Identity;

namespace ContactsManager.Infrastructure.Identity;

public static class RoleSeeder
{
    private static readonly string[] Roles = Enum.GetNames<Role>();

    public static async Task SeedAsync(RoleManager<AppRole> roleManager)
    {
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new AppRole { Name = role });
        }
    }
}
