using GymManagementSystem.Data;
using GymManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GymManagementSystem.Controllers
{

    [Authorize(Roles = "Receptionist")]
    public class PaymentsController : Controller
    {

        private readonly ApplicationDbContext _context;


        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }




        // View payment history
        public async Task<IActionResult> Index()
        {

            var payments = await _context.Payments
                .Include(p => p.Member)
                .Include(p => p.Membership)
                .ThenInclude(m => m.MembershipPlan)
                .ToListAsync();


            return View(payments);

        }








        // Open record payment page
        [HttpGet]
        public async Task<IActionResult> Create(int? memberId)
        {

            if(memberId == null)
            {
                return NotFound();
            }



            var member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == memberId);



            if(member == null)
            {
                return NotFound();
            }



            ViewBag.Member = member;



            ViewBag.Memberships = await _context.Memberships
                .Include(m => m.MembershipPlan)
                .Where(m => m.MemberId == memberId)
                .ToListAsync();





            var payment = new Payment
            {
                MemberId = memberId.Value,
                PaymentDate = DateTime.Now,
                Status = "Paid"
            };



            return View(payment);

        }









        // Save payment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {


            if(ModelState.IsValid)
            {

                _context.Payments.Add(payment);


                await _context.SaveChangesAsync();



                return RedirectToAction(nameof(Index));

            }





            ViewBag.Member = await _context.Members
                .FirstOrDefaultAsync(m => m.Id == payment.MemberId);



            ViewBag.Memberships = await _context.Memberships
                .Include(m => m.MembershipPlan)
                .Where(m => m.MemberId == payment.MemberId)
                .ToListAsync();



            return View(payment);

            

        }








        // Check payment status
        public async Task<IActionResult> Status()
        {

            var payments = await _context.Payments
                .Include(p => p.Member)
                .Include(p => p.Membership)
                .ThenInclude(m => m.MembershipPlan)
                .ToListAsync();



            return View(payments);

        }



    }

}