using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using GymManagementSystem.Models;

namespace GymManagementSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }

    }
}