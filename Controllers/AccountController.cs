using System.Security.Claims;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
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
            var profile = _context.UserProfiles.FirstOrDefault(p => p.Email == email);

            if (profile == null || !VerifyPassword(profile, model.Password))
            {
                ModelState.AddModelError(string.Empty, "Email ama password khaldan.");
                return View(model);
            }

            if (!string.Equals(profile.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(string.Empty, "Akoonkaagu waa la joojiyay. La xidhiidh maamulka.");
                return View(model);
            }

            profile.LastLogin = DateTime.UtcNow;
            _context.SaveChanges();

            await SignInAsync(profile, model.RememberMe);

            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToRoleDashboard(profile.Role);
        }

        // =========================================================
        // 2. REGISTER (guests always join as Member)
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

            var id = Guid.NewGuid().ToString();
            var name = model.FullName.Trim();
            var avatar = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(name)}&background=a3e635&color=000";

            var profile = new UserProfile
            {
                Id = id,
                Name = name,
                Email = email,
                Username = email,
                Role = UserRoles.Member,
                Status = "Active",
                JoinDate = DateTime.UtcNow,
                LastLogin = DateTime.UtcNow,
                Phone = string.Empty,
                Address = string.Empty,
                DateOfBirth = string.Empty,
                Gender = string.Empty,
                Bio = string.Empty,
                Avatar = avatar,
                MembershipType = "Basic",
                Specialization = string.Empty,
                Shift = string.Empty
            };
            profile.Password = _passwordHasher.HashPassword(profile, model.Password);

            _context.UserProfiles.Add(profile);
            _context.Users.Add(new User
            {
                Id = id,
                Name = name,
                Role = UserRoles.Member,
                Avatar = avatar,
                Status = "Offline",
                LastSeen = DateTime.UtcNow,
                Specialization = string.Empty,
                MembershipType = "Basic",
                Shift = string.Empty
            });
            _context.SaveChanges();

            await SignInAsync(profile, isPersistent: false);

            return RedirectToRoleDashboard(profile.Role);
        }

        // =========================================================
        // 3. LOGOUT / ACCESS DENIED
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("HomePage", "Guest");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
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

            var result = _passwordHasher.VerifyHashedPassword(profile, profile.Password, password);

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                profile.Password = _passwordHasher.HashPassword(profile, password);
                _context.SaveChanges();
            }

            return result != PasswordVerificationResult.Failed;
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
