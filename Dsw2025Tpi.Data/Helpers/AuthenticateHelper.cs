using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Data.Helpers;

public static class AuthenticateHelper
{
    public static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
    {
        string[] roleNames = { "Admin", "User" };

        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }
}