using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MembershipPlansController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembershipPlansController(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var plans =
                await _context.MembershipPlans
                    .OrderBy(p => p.Price)
                    .ToListAsync();

            return View(plans);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            MembershipPlan plan)
        {
            if (!ModelState.IsValid)
            {
                return View(plan);
            }

            _context.MembershipPlans.Add(plan);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan =
                await _context.MembershipPlans
                    .FindAsync(id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            MembershipPlan plan)
        {
            if (id != plan.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(plan);
            }

            _context.Update(plan);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var plan =
                await _context.MembershipPlans
                    .FirstOrDefaultAsync(
                        p => p.Id == id);

            if (plan == null)
            {
                return NotFound();
            }

            return View(plan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id)
        {
            var plan =
                await _context.MembershipPlans
                    .FindAsync(id);

            if (plan != null)
            {
                _context.MembershipPlans.Remove(plan);

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}