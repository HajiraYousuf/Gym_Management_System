using Microsoft.AspNetCore.Identity;
using GymManagementSystem.Models;

namespace GymManagementSystem.Seed
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            string adminRole = "Admin";

            // Make sure Admin role exists
            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(adminRole));
            }


            string adminEmail = "admin@gym.com";
            string adminPassword = "Admin@123";


            var adminUser = await userManager.FindByEmailAsync(adminEmail);


            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Gym Administrator",
                    EmailConfirmed = true
                };


                await userManager.CreateAsync(
                    adminUser,
                    adminPassword);


                await userManager.AddToRoleAsync(
                    adminUser,
                    adminRole);
            }
        }
    }
}