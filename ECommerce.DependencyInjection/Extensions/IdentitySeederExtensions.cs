using ECommerce.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.DependencyInjection.Extensions
{
    public static class IdentitySeederExtensions
    {
        public static async Task SeedIdentityDataAsync(this IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            // Create roles
            var roles = new[]
            {
                new { Name = "SuperAdmin", Description = "Super Administrator with full access" },
                new { Name = "Admin", Description = "Administrator with management access" },
                new { Name = "Customer", Description = "Customer with limited access" }
            };

            foreach (var roleData in roles)
            {
                if (!await roleManager.RoleExistsAsync(roleData.Name))
                {
                    await roleManager.CreateAsync(new Role
                    {
                        Name = roleData.Name,
                        Description = roleData.Description,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            // Create SuperAdmin user
            var superAdminEmail = "superadmin@ecommerce.com";
            var superAdminUser = await userManager.FindByEmailAsync(superAdminEmail);
            if (superAdminUser == null)
            {
                superAdminUser = new User
                {
                    UserName = "SuperAdmin",
                    Email = superAdminEmail,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                await userManager.CreateAsync(superAdminUser, "SuperAdmin123!");
                await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
            }
        }
    }
}
