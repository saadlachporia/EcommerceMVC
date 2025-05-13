using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using EcommerceMVC.Models;
using System.Threading.Tasks;

namespace EcommerceMVC.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null || !user.IsAdmin)
            {
                return Forbid(); // 403 Forbidden if user is not an admin
            }

            // Return the admin dashboard view
            return View();
        }
    }
}
