using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Receptionist")]
    public class MembersController : Controller
    {

        private readonly ApplicationDbContext _context;


        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }





        // Display all members
        public async Task<IActionResult> Index()
        {
            var members = await _context.Members.ToListAsync();

            return View(members);
        }





        // Open Add Member form
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }





        // Save new member
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member)
        {

            if (ModelState.IsValid)
            {

                // Add member
                _context.Members.Add(member);

                await _context.SaveChangesAsync();




                // Add activity log
                var activity = new ActivityLog
                {
                    Description = $"New member registered: {member.FullName}",
                    Date = DateTime.Now
                };


                _context.ActivityLogs.Add(activity);

                await _context.SaveChangesAsync();




                return RedirectToAction("Index");
            }


            return View(member);
        }






        // Open Edit Member form
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }


            var member = await _context.Members.FindAsync(id);


            if (member == null)
            {
                return NotFound();
            }


            return View(member);

        }





        // Save edited member
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {

            if (id != member.Id)
            {
                return NotFound();
            }


            if (ModelState.IsValid)
            {

                _context.Update(member);

                await _context.SaveChangesAsync();


                return RedirectToAction("Index");

            }


            return View(member);

        }







        // Show delete confirmation page
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }


            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == id);


            if (member == null)
            {
                return NotFound();
            }


            return View(member);

        }







        // Delete member
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {

            var member = await _context.Members.FindAsync(id);


            if (member != null)
            {

                _context.Members.Remove(member);

                await _context.SaveChangesAsync();

            }


            return RedirectToAction("Index");

        }


    }
}