using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers
{
    [Authorize(Roles = "Receptionist")]
    public class ReceptionistController : Controller
    {

        private readonly ApplicationDbContext _context;



        public ReceptionistController(ApplicationDbContext context)
        {
            _context = context;
        }





        public async Task<IActionResult> Index()
        {
            return View();
        }






        [HttpGet]
        public IActionResult AddMember()
        {
            return View();
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(Member member)
        {

            if (ModelState.IsValid)
            {

                _context.Members.Add(member);

                await _context.SaveChangesAsync();




                // Create activity record
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



    }
}