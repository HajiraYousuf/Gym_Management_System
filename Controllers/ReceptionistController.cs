using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers
{
    public class ReceptionistController : Controller
    {
        // 1. CheckInOut
        public IActionResult CheckInOut()
        {
            return View();
        }

        // 2. ClassSchedule
        public IActionResult ClassSchedule()
        {
            return View();
        }

        // 3. Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // 4. Members
        public IActionResult Members()
        {
            return View();
        }

        // 5. Memberships
        public IActionResult Memberships()
        {
            return View();
        }

        // 6. Orders
        public IActionResult Orders()
        {
            return View();
        }

        // 7. Payments
        public IActionResult Payments()
        {
            return View();
        }

        // 8. Report
        public IActionResult Report()
        {
            return View();
        }

        // 9. Reservation
        public IActionResult Reservation()
        {
            return View();
        }

        // 10. VisitorLog
        public IActionResult VisitorLog()
        {
            return View();
        }
    }
}