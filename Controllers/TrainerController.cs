using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers
{
    public class TrainerController : Controller
    {
        // 1. ClassAttendance
        public IActionResult ClassAttendance()
        {
            return View();
        }

        // 2. ClassesSchedule
        public IActionResult ClassesSchedule()
        {
            return View();
        }

        // 3. Dashboard
        public IActionResult Dashboard()
        {
            return View();
        }

        // 4. MemberDetail
        public IActionResult MemberDetail()
        {
            return View();
        }

        // 5. Members
        public IActionResult Members()
        {
            return View();
        }

        // 6. NutritionPlans
        public IActionResult NutritionPlans()
        {
            return View();
        }

        // 7. Report
        public IActionResult Report()
        {
            return View();
        }

        // 8. WorkoutPlans
        public IActionResult WorkoutPlans()
        {
            return View();
        }
    }
}