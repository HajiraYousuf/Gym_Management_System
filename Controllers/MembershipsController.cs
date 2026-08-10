using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Receptionist")]
    public class MembershipsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembershipsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Display all memberships
        public async Task<IActionResult> Index()
        {
            var memberships = await _context.Memberships
                .Include(m => m.Member)
                .Include(m => m.MembershipPlan)
                .ToListAsync();

            return View(memberships);
        }

        // Show assign membership page
        [HttpGet]
        public async Task<IActionResult> Create(int? memberId)
        {
            if (memberId == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == memberId);

            if (member == null)
            {
                return NotFound();
            }

            ViewBag.Member = member;

            ViewBag.Plans = await _context.MembershipPlans
                .ToListAsync();

            var membership = new Membership
            {
                MemberId = member.Id,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                Status = "Active"
            };

            return View(membership);
        }

        // Save assigned membership
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Membership membership)
        {
            if (ModelState.IsValid)
            {
                membership.Member = await _context.Members
                    .FirstOrDefaultAsync(m => m.Id == membership.MemberId);

                membership.MembershipPlan = await _context.MembershipPlans
                    .FirstOrDefaultAsync(p => p.Id == membership.MembershipPlanId);

                if (membership.Member == null || membership.MembershipPlan == null)
                {
                    return NotFound();
                }

                membership.Status = "Active";

                // Calculate end date based on selected membership plan
                membership.StartDate = DateTime.Now;

                membership.EndDate = DateTime.Now
                    .AddDays(membership.MembershipPlan.DurationInDays);

                _context.Memberships.Add(membership);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == membership.MemberId);

            ViewBag.Plans = await _context.MembershipPlans
                .ToListAsync();

            return View(membership);
        }

        // Show renew page
        [HttpGet]
        public async Task<IActionResult> Renew(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var membership = await _context.Memberships
                .Include(m => m.Member)
                .Include(m => m.MembershipPlan)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (membership == null)
            {
                return NotFound();
            }

            return View(membership);
        }

        // Save renewal
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renew(int id)
        {
            var membership = await _context.Memberships
                .Include(m => m.MembershipPlan)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (membership == null)
            {
                return NotFound();
            }

            if (membership.MembershipPlan == null)
            {
                return NotFound();
            }

            membership.StartDate = DateTime.Now;

            membership.EndDate = DateTime.Now
                .AddDays(membership.MembershipPlan.DurationInDays);

            membership.Status = "Active";

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Show expired memberships
        public async Task<IActionResult> Expired()
        {
            var expiredMemberships = await _context.Memberships
                .Include(m => m.Member)
                .Include(m => m.MembershipPlan)
                .Where(m => m.EndDate < DateTime.Now)
                .ToListAsync();

            return View(expiredMemberships);
        }
    }
}