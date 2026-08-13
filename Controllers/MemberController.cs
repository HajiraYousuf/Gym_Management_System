using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers
{
    public class MemberController : Controller
    {
        // 1. Attendance
        public IActionResult Attendance()
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

        // 4. Membership
        public IActionResult Membership()
        {
            return View();
        }

        // 5. NutritionPlans
        public IActionResult NutritionPlans()
        {
            return View();
        }

        // 6. Progress
        public IActionResult Progress()
        {
            return View();
        }

        // 7. WorkoutPlans
        public IActionResult WorkoutPlans()
        {
            return View();
        }
    }
}