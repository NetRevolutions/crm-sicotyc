using Jarasoft.Sicotyc.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace Jarasoft.Sicotyc.Infrastructure.Identity
{
    public static class IdentitySeeder
    {
        public static async Task SeedRolesAsync(
          IServiceProvider serviceProvider)
        { 
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

            foreach (var roleName in ApplicationRoles.All)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(
                    new ApplicationRole 
                    {
                        Id = Guid.NewGuid(),
                        Name = roleName
                    });

                if (!result.Succeeded)
                { 
                    var errors = string.Join(
                        "; ", 
                        result.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"No se pudo crear el role '{roleName}'. Errores: {errors}");
                }
            }
        }
    }
}
