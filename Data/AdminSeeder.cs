using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using GymManagementSystem.Models;

namespace GymManagementSystem.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration configuration)
        {
            // Read credentials from config
            var adminEmail = configuration["AdminUser:Email"];
            var adminPassword = configuration["AdminUser:Password"];

            if (string.IsNullOrEmpty(adminEmail) || string.IsNullOrEmpty(adminPassword))
            {
                // Nothing configured; skip seeding
                return;
            }

            // If any user is already in Admin role, skip
            var usersInAdmin = await userManager.GetUsersInRoleAsync("Admin");
            if (usersInAdmin != null && usersInAdmin.Count > 0)
            {
                return;
            }

            // If a user with the configured email exists, ensure they have Admin role
            var existing = await userManager.FindByEmailAsync(adminEmail);
            if (existing != null)
            {
                var roles = await userManager.GetRolesAsync(existing);
                if (!roles.Contains("Admin"))
                {
                    await userManager.AddToRoleAsync(existing, "Admin");
                }
                return;
            }

            // Create the admin user
            var adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FullName = "Administrator"
            };

            var result = await userManager.CreateAsync(adminUser, adminPassword);
            if (result.Succeeded)
            {
                if (!await roleManager.RoleExistsAsync("Admin"))
                {
                    await roleManager.CreateAsync(new IdentityRole("Admin"));
                }
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
            // If user creation fails, do not crash startup; errors will appear in logs.
        }
    }
}
