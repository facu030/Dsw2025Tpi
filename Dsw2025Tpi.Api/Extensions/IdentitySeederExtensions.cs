using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Dsw2025Tpi.Api.Extensions
{
    public static class IdentitySeederExtensions
    {
        public static async Task UseIdentitySeeding(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<Dsw2025TpiContext>();

            // Verifica si la base de datos es accesible
            if (!await db.Database.CanConnectAsync())
            {
                Console.WriteLine("⚠️ Base de datos no disponible. Saltando Identity Seeding.");
                return;
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            await SeedRoles(roleManager);
            await SeedAdmin(userManager, roleManager);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = new[] { "Admin", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new RoleSeedingException($"Error al crear el rol '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }

        private static async Task SeedAdmin(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var adminEmail = "admin@system.com";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new IdentityUser
                {
                    UserName = "admin",
                    Email = adminEmail
                };

                var createResult = await userManager.CreateAsync(adminUser, "Admin123!");

                if (!createResult.Succeeded)
                {
                    throw new UserSeedingException(
                        $"Error al crear el usuario admin: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                }

                var roleResult = await userManager.AddToRoleAsync(adminUser, "Admin");

                if (!roleResult.Succeeded)
                {
                    throw new UserSeedingException(
                        $"Error al asignar el rol 'Admin' al usuario admin: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");

                }
            }
        }
    }
}
