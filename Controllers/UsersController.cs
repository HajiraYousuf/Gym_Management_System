using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UsersController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();

            var userRoles =
                new Dictionary<string, IList<string>>();

            foreach (var user in users)
            {
                userRoles[user.Id] =
                    await _userManager.GetRolesAsync(user);
            }

            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Roles = GetRoles();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            ApplicationUser user,
            string selectedRole,
            string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError(
                    "Password",
                    "Password is required.");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = GetRoles();
                return View(user);
            }

            user.UserName = user.Email;

            var result =
                await _userManager.CreateAsync(
                    user,
                    password);

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(selectedRole))
                {
                    await _userManager.AddToRoleAsync(
                        user,
                        selectedRole);
                }

                return RedirectToAction(nameof(Index));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    "",
                    error.Description);
            }

            ViewBag.Roles = GetRoles();

            return View(user);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles =
                await _userManager.GetRolesAsync(user);

            ViewBag.Roles = GetRoles();
            ViewBag.CurrentRole =
                roles.FirstOrDefault();

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            string id,
            ApplicationUser model,
            string selectedRole)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = GetRoles();
                return View(model);
            }

            user.FullName = model.FullName;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            var updateResult =
                await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                ViewBag.Roles = GetRoles();
                return View(model);
            }

            var currentRoles =
                await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);
            }

            if (!string.IsNullOrWhiteSpace(selectedRole))
            {
                await _userManager.AddToRoleAsync(
                    user,
                    selectedRole);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles =
                await _userManager.GetRolesAsync(user);

            ViewBag.UserRoles = roles;

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            string id)
        {
            var user =
                await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result =
                await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "",
                        error.Description);
                }

                return View("Delete", user);
            }

            return RedirectToAction(nameof(Index));
        }

        private List<string> GetRoles()
        {
            return new List<string>
            {
                "Admin",
                "Receptionist",
                "Trainer",
                "Member"
            };
        }
    }
}