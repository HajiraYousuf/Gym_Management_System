using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GymManagementSystem.Controllers
{
    //[Authorize(Roles = "Trainer")]
    public class TrainerController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}