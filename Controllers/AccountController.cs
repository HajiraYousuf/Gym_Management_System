using System.Security.Claims;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PasswordHasher<UserProfile> _passwordHasher = new();

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
            // NOTE: Admin seeding removed from here on purpose.
            // A new AccountController instance is created on EVERY request in ASP.NET Core.
            // Doing the "does admin exist? -> insert admin" check here meant that two
            // concurrent requests could both see "no admin yet" and both try to INSERT it,
            // which throws a UNIQUE KEY constraint violation once the Email index is unique
            // (see the AddAccountAuthConstraints migration). Seeding now happens exactly
            // once at application startup via AccountSeedData.SeedAccounts(app.Services)
            // in Program.cs — see Program_SeedingFix_Snippet.cs.
        }

        // =========================================================
        // 1. LOGIN
        // =========================================================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleDashboard(User.FindFirstValue(ClaimTypes.Role));
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();

            var profile = await _context.UserProfiles
                .FirstOrDefaultAsync(p => p.Email == email);

            if (profile == null)
            {
                ModelState.AddModelError(string.Empty,
                    "Email ama password ayaa khaldan.");
                return View(model);
            }

            var passwordResult = _passwordHasher.VerifyHashedPassword(
                profile,
                profile.Password,
                model.Password
            );

            if (passwordResult == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(string.Empty,
                    "Email ama password ayaa khaldan.");
                return View(model);
            }

            if (!string.Equals(profile.Status, "Active",
                StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty,
                    "Akoonkaagu ma Active aha.");
                return View(model);
            }

            profile.LastLogin = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await SignInAsync(profile, model.RememberMe);

            if (!string.IsNullOrWhiteSpace(returnUrl) &&
                Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToRoleDashboard(profile.Role);
        }
        // =========================================================
        // 2. REGISTER
        // Guest-ku wuxuu is-diiwaangelinayaa Role = Guest, Status = Active.
        // Ma codsanayo membership wakhtigan — wuxuu si caadi ah u geli karaa
        // sida guest, oo goor dambe ka codsan kara membership (dhis 4).
        // =========================================================
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleDashboard(User.FindFirstValue(ClaimTypes.Role));
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var email = model.Email.Trim();

            if (_context.UserProfiles.Any(p => p.Email == email))
            {
                ModelState.AddModelError(nameof(model.Email), "Email-kan horey ayaa loo isticmaalay.");
                return View(model);
            }

            // Isticmaal Database Transaction si labada miis isku mar wax loogu daro
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var guestCount = _context.UserProfiles.Count(p => p.Role == UserRoles.Guest);
                var id = $"GU-{guestCount + 1:D3}";
                var name = model.FullName.Trim();
                var avatar = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=a3e635&color=000";

                var profile = new UserProfile
                {
                    Id = id,
                    Name = name,
                    Email = email,
                    Username = email,
                    Role = UserRoles.Guest,
                    Status = "Active",
                    MembershipRequested = false,
                    JoinDate = DateTime.UtcNow,
                    LastLogin = DateTime.UtcNow,
                    Phone = string.Empty,
                    Address = string.Empty,
                    DateOfBirth = string.Empty,
                    Gender = string.Empty,
                    Bio = string.Empty,
                    Avatar = avatar,
                    MembershipType = string.Empty,
                    Specialization = string.Empty,
                    Shift = string.Empty
                };
                profile.Password = _passwordHasher.HashPassword(profile, model.Password);

                _context.UserProfiles.Add(profile);

                _context.Users.Add(new User
                {
                    Id = id, // Hubi inuu yahay kan isla eg ee UserProfile
                    Name = name,
                    Role = UserRoles.Guest,
                    Avatar = avatar,
                    Status = "Offline",
                    LastSeen = DateTime.UtcNow,
                    Specialization = string.Empty,
                    MembershipType = string.Empty,
                    Shift = string.Empty
                });

                _context.SaveChanges();
                transaction.Commit(); // Haddii wax walba sax yihiin kaydi

                await SignInAsync(profile, isPersistent: false);

                return RedirectToRoleDashboard(profile.Role);
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Haddii qalad dhaco dib u celi
                ModelState.AddModelError(string.Empty, "Cillad ayaa dhacday markii la diiwaangelinayay: " + ex.Message);
                return View(model);
            }
        }
        // =========================================================
        // 3. MEMBERSHIP REQUEST (Guest -> wants to become Member)
        // Guest-ku goor uu doono ayuu ka codsan karaa membership.
        // Admin ama Receptionist ayaa ansixin kara (eeg AdminController /
        // ReceptionistController -> ApproveMembership).
        // =========================================================
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RequestMembership()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = _context.UserProfiles.FirstOrDefault(p => p.Id == userId);

            if (profile == null || profile.Role != UserRoles.Guest)
            {
                return Forbid();
            }

            profile.MembershipRequested = true;
            _context.SaveChanges();

            TempData["Message"] = "Codsigaaga membership-ka waa la diray. Fadlan sug ansixinta Admin ama Reception.";
            return RedirectToAction(nameof(PendingApproval));
        }

        // =========================================================
        // 4. LOGOUT / ACCESS DENIED / PENDING APPROVAL
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("HomePage", "Guest");
        }

        [HttpGet]
        public async Task<IActionResult> LogoutGet()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("HomePage", "Guest");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PendingApproval()
        {
            // Guest-ka oo aan weli la ansixin (role weli waa Guest) ayaa
            // halkan imanaya. Haddii uu horey u noqday Member/Trainer/etc,
            // waxaa loo wareejinayaa dashboard-kiisa saxda ah.
            if (User.FindFirstValue(ClaimTypes.Role) != UserRoles.Guest)
            {
                return RedirectToRoleDashboard(User.FindFirstValue(ClaimTypes.Role));
            }
            return View();
        }

        // =========================================================
        // HELPERS
        // =========================================================
        private bool VerifyPassword(UserProfile profile, string password)
        {
            if (string.IsNullOrEmpty(profile.Password))
            {
                return false;
            }

            try
            {
                // 1. Isku day inuu hubiyo Identity Hash-ka caadiga ah
                var result = _passwordHasher.VerifyHashedPassword(profile, profile.Password, password);

                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    profile.Password = _passwordHasher.HashPassword(profile, password);
                    _context.SaveChanges();
                }

                return result != PasswordVerificationResult.Failed;
            }
            catch (FormatException)
            {
                // 2. Haddii uu database-ka ku jiro Plain Text (Base-64 Error uu dhaco)
                if (profile.Password == password)
                {
                    // Si otomaatig ah ugu beddel Password-ka mid Hash ah si uusan error dambe u dhicin
                    profile.Password = _passwordHasher.HashPassword(profile, password);
                    _context.SaveChanges();

                    return true;
                }

                return false;
            }
        }
        private async Task SignInAsync(UserProfile profile, bool isPersistent)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, profile.Id),
                new(ClaimTypes.Name, profile.Name),
                new(ClaimTypes.Email, profile.Email),
                new(ClaimTypes.Role, profile.Role),
                new("Avatar", profile.Avatar ?? string.Empty)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity),
                new AuthenticationProperties
                {
                    IsPersistent = isPersistent,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(isPersistent ? 14 : 1)
                });
        }

        private IActionResult RedirectToRoleDashboard(string? role)
        {
            return RedirectToAction(UserRoles.DashboardAction(role), UserRoles.DashboardController(role));
        }
    }
}
