using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // =========================================================
        // ADMIN DASHBOARD
        // =========================================================

        public IActionResult Dashboard()
        {
            
            return View();
        }
        public async Task<IActionResult> Index()
        {
            var totalMembers = await _context.Members.CountAsync();

            var trainers = await _userManager.GetUsersInRoleAsync("Trainer");
            var totalTrainers = trainers.Count;

            var activeMemberships = await _context.Memberships
                .CountAsync(m => m.Status == "Active");

            var recentActivities = await _context.ActivityLogs
                .OrderByDescending(a => a.Date)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalMembers = totalMembers;
            ViewBag.TotalTrainers = totalTrainers;
            ViewBag.ActiveMemberships = activeMemberships;
            ViewBag.RecentActivities = recentActivities;

            return View();
        }

        // =========================================================
        // USER MANAGEMENT
        // =========================================================

        // View all users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();

            var model = new List<UserListViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                model.Add(new UserListViewModel
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles
                });
            }

            return View(model);
        }

        // ---------------------------------------------------------
        // ADD USER
        // ---------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            var roles = await _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .Select(r => r!)
                .ToListAsync();

            var model = new UserCreateViewModel
            {
                Roles = roles
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(
            UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await _roleManager.Roles
                    .Select(r => r.Name)
                    .Where(r => r != null)
                    .Select(r => r!)
                    .ToListAsync();

                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password
            );

            if (result.Succeeded)
            {
                if (!string.IsNullOrWhiteSpace(model.SelectedRole))
                {
                    await _userManager.AddToRoleAsync(
                        user,
                        model.SelectedRole
                    );
                }

                return RedirectToAction(nameof(Users));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }

            model.Roles = await _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .Select(r => r!)
                .ToListAsync();

            return View(model);
        }

        // ---------------------------------------------------------
        // EDIT USER
        // ---------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> EditUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _roleManager.Roles
                .Select(r => r.Name)
                .Where(r => r != null)
                .Select(r => r!)
                .ToListAsync();

            var currentRoles = await _userManager.GetRolesAsync(user);

            var model = new UserEditViewModel
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FullName = user.FullName ?? string.Empty,
                Roles = roles,
                SelectedRole = currentRoles.FirstOrDefault() ?? string.Empty
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(
            UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = await _roleManager.Roles
                    .Select(r => r.Name)
                    .Where(r => r != null)
                    .Select(r => r!)
                    .ToListAsync();

                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            user.Email = model.Email;
            user.UserName = model.Email;
            user.FullName = model.FullName;

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );
                }

                model.Roles = await _roleManager.Roles
                    .Select(r => r.Name)
                    .Where(r => r != null)
                    .Select(r => r!)
                    .ToListAsync();

                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles
                );
            }

            if (!string.IsNullOrWhiteSpace(model.SelectedRole))
            {
                await _userManager.AddToRoleAsync(
                    user,
                    model.SelectedRole
                );
            }

            return RedirectToAction(nameof(Users));
        }

        // ---------------------------------------------------------
        // DELETE USER
        // ---------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description
                    );
                }
            }

            return RedirectToAction(nameof(Users));
        }

        // =========================================================
        // MEMBERSHIP PLAN MANAGEMENT
        // =========================================================

        // View available membership plans
        public async Task<IActionResult> MembershipPlans()
        {
            var plans = await _context.MembershipPlans
                .ToListAsync();

            return View(plans);
        }

        // ---------------------------------------------------------
        // CREATE MEMBERSHIP PLAN
        // ---------------------------------------------------------

        [HttpGet]
        public IActionResult CreatePlan()
        {
            return View(new MembershipPlan());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePlan(
            MembershipPlan model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            _context.MembershipPlans.Add(model);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MembershipPlans));
        }

        // ---------------------------------------------------------
        // EDIT MEMBERSHIP PLAN
        // ---------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> EditPlan(int id)
        {
            var plan = await _context.MembershipPlans
                .FindAsync(id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPlan(
            MembershipPlan model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingPlan = await _context.MembershipPlans
                .FindAsync(model.Id);

            if (existingPlan == null)
            {
                return NotFound();
            }

            existingPlan.PlanName = model.PlanName;
            existingPlan.Price = model.Price;
            existingPlan.DurationInDays = model.DurationInDays;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MembershipPlans));
        }

        // ---------------------------------------------------------
        // DELETE MEMBERSHIP PLAN
        // ---------------------------------------------------------

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePlan(int id)
        {
            var plan = await _context.MembershipPlans
                .FindAsync(id);

            if (plan == null)
            {
                return NotFound();
            }

            _context.MembershipPlans.Remove(plan);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MembershipPlans));
        }
    }
}