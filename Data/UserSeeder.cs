using Microsoft.AspNetCore.Identity;
using GymManagementSystem.Models;

namespace GymManagementSystem.Data
{
    public static class UserSeeder
    {
        public static async Task SeedUsersAsync(
            UserManager<ApplicationUser> userManager)
        {


            // Receptionist User

            var receptionistEmail = "receptionist@gym.com";


            var receptionist = await userManager.FindByEmailAsync(
                receptionistEmail
            );


            if (receptionist == null)
            {
                receptionist = new ApplicationUser
                {
                    UserName = receptionistEmail,
                    Email = receptionistEmail,
                    FullName = "Gym Receptionist",
                    EmailConfirmed = true
                };


                await userManager.CreateAsync(
                    receptionist,
                    "Password123!"
                );


                await userManager.AddToRoleAsync(
                    receptionist,
                    "Receptionist"
                );
            }





            // Trainer User

            var trainerEmail = "trainer@gym.com";


            var trainer = await userManager.FindByEmailAsync(
                trainerEmail
            );


            if (trainer == null)
            {
                trainer = new ApplicationUser
                {
                    UserName = trainerEmail,
                    Email = trainerEmail,
                    FullName = "Gym Trainer",
                    EmailConfirmed = true
                };


                await userManager.CreateAsync(
                    trainer,
                    "Password123!"
                );


                await userManager.AddToRoleAsync(
                    trainer,
                    "Trainer"
                );
            }





            // Member User

            var memberEmail = "member@gym.com";


            var member = await userManager.FindByEmailAsync(
                memberEmail
            );


            if (member == null)
            {
                member = new ApplicationUser
                {
                    UserName = memberEmail,
                    Email = memberEmail,
                    FullName = "Gym Member",
                    EmailConfirmed = true
                };


                await userManager.CreateAsync(
                    member,
                    "Password123!"
                );


                await userManager.AddToRoleAsync(
                    member,
                    "Member"
                );
            }





            // Admin User

            var adminEmail = "admin@gym.com";


            var admin = await userManager.FindByEmailAsync(
                adminEmail
            );


            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Gym Administrator",
                    EmailConfirmed = true
                };


                await userManager.CreateAsync(
                    admin,
                    "Admin@12345"
                );


                await userManager.AddToRoleAsync(
                    admin,
                    "Admin"
                );
            }


        }
    }
}