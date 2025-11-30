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

            //la BD de Identity exista con sus tablas
            var identityContext = scope.ServiceProvider.GetRequiredService<AuthenticateContext>();
            await identityContext.Database.EnsureCreatedAsync();

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await SeedRoles(roleManager);
        }

        private static async Task SeedRoles(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = new[] { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));

                    if (!result.Succeeded)
                    {
                        throw new RoleSeedingException(
                            $"Error al crear el rol '{role}': {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }
        }
    }
}