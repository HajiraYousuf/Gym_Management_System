using GymManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GymManagementSystem.Data
{
    /// <summary>
    /// Applies pending migrations and creates one default account per role.
    /// Passwords are hashed, never stored in clear text.
    /// This is the ONLY place default accounts get seeded — do not
    /// duplicate this logic in a controller constructor.
    /// </summary>
    public static class AccountSeedData
    {
        public const string DefaultPassword = "Password@123";

        private static readonly (string Id, string Name, string Email, string Role, string Extra)[] DefaultAccounts =
        {
            ("admin1", "System Admin", "admin@ironcore.com", UserRoles.Admin, "Morning"),
            ("trainer1", "Ahmed Trainer", "trainer@ironcore.com", UserRoles.Trainer, "Strength & Conditioning"),
            ("reception1", "Hodan Reception", "reception@ironcore.com", UserRoles.Receptionist, "Morning"),
            ("member1", "Ayaan Member", "member@ironcore.com", UserRoles.Member, "Basic")
        };

        public static void SeedAccounts(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Database.Migrate();

            var hasher = new PasswordHasher<UserProfile>();

            foreach (var (id, name, email, role, extra) in DefaultAccounts)
            {
                if (context.UserProfiles.Any(p => p.Email == email))
                {
                    continue;
                }

                var profile = new UserProfile
                {
                    Id = id,
                    Name = name,
                    Email = email,
                    Username = email,
                    Role = role,
                    Status = "Active",
                    MembershipRequested = false, // default accounts never sit in the pending-approval flow
                    JoinDate = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    Phone = string.Empty,
                    Address = string.Empty,
                    DateOfBirth = string.Empty,
                    Gender = string.Empty,
                    Bio = string.Empty,
                    Avatar = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=a3e635&color=000",
                    MembershipType = role == UserRoles.Member ? extra : string.Empty,
                    Specialization = role == UserRoles.Trainer ? extra : string.Empty,
                    Shift = role is UserRoles.Admin or UserRoles.Receptionist ? extra : string.Empty
                };
                profile.Password = hasher.HashPassword(profile, DefaultPassword);
                context.UserProfiles.Add(profile);

                if (!context.Users.Any(u => u.Id == id))
                {
                    context.Users.Add(new User
                    {
                        Id = id,
                        Name = name,
                        Role = role,
                        Avatar = profile.Avatar,
                        Status = "Offline",
                        LastSeen = DateTime.UtcNow,
                        Specialization = profile.Specialization,
                        MembershipType = profile.MembershipType,
                        Shift = profile.Shift
                    });
                }
            }

            context.SaveChanges();
        }
    }
}
