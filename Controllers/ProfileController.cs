using EcommerceMVC.Data;
using EcommerceMVC.Models;
using EcommerceMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace EcommerceMVC.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileController(ApplicationDbContext context,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // GET: Profile
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
                          ?? new UserProfile { UserId = userId ?? string.Empty };

            var addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
            var paymentMethods = await _context.PaymentMethods.Where(p => p.UserId == userId).ToListAsync();

            var model = new ProfileViewModel
            {
                Profile = profile,
                Addresses = addresses,
                PaymentMethods = paymentMethods,
                NewAddress = new Address(),
                NewPaymentMethod = new PaymentMethod(),
                PasswordModel = new ChangePasswordViewModel()
            };

            return View(model);
        }

        // POST: Update Profile Info
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(ProfileViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId);

            if (existing == null)
            {
                model.Profile.UserId = userId ?? string.Empty;
                _context.UserProfiles.Add(model.Profile);
            }
            else
            {
                existing.FullName = model.Profile.FullName;
                existing.PhoneNumber = model.Profile.PhoneNumber;
                existing.Email = model.Profile.Email;
            }

            await _context.SaveChangesAsync();
            TempData["ProfileUpdated"] = "Profile updated successfully!";
            return RedirectToAction("Index");
        }

        // POST: Add new Address
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(ProfileViewModel model)
        {
            model.NewAddress.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            _context.Addresses.Add(model.NewAddress);
            await _context.SaveChangesAsync();
            TempData["AddressAdded"] = "Address added successfully!";
            return RedirectToAction("Index");
        }

        // POST: Add new Payment Method
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPaymentMethod(ProfileViewModel model)
        {
            model.NewPaymentMethod.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? string.Empty;
            _context.PaymentMethods.Add(model.NewPaymentMethod);
            await _context.SaveChangesAsync();
            TempData["CardAdded"] = "Payment method added successfully!";
            return RedirectToAction("Index");
        }

        // POST: Change Password
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return await ReloadProfileView(model.PasswordModel);

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "Account");

            var result = await _userManager.ChangePasswordAsync(user,
                model.PasswordModel.CurrentPassword, model.PasswordModel.NewPassword);

            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Message"] = "Password changed successfully.";
                return RedirectToAction("Index");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return await ReloadProfileView(model.PasswordModel);
        }

        // POST: Delete Address
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteAddress(int id)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

    if (address != null)
    {
        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        TempData["AddressDeleted"] = "Address removed successfully.";
    }

    return RedirectToAction("Index");
}

// POST: Delete Payment Method
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeletePaymentMethod(int id)
{
    var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
    var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);

    if (paymentMethod != null)
    {
        _context.PaymentMethods.Remove(paymentMethod);
        await _context.SaveChangesAsync();
        TempData["CardDeleted"] = "Payment method removed successfully.";
    }

    return RedirectToAction("Index");
}


        // Helper to reload profile view with proper data and password model for error display
        private async Task<IActionResult> ReloadProfileView(ChangePasswordViewModel passwordModel)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var profile = await _context.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId)
                ?? new UserProfile { UserId = userId };

            var addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
            var paymentMethods = await _context.PaymentMethods.Where(p => p.UserId == userId).ToListAsync();

            var model = new ProfileViewModel
            {
                Profile = profile,
                PasswordModel = passwordModel,
                PaymentMethods = paymentMethods,
                Addresses = addresses,
                NewAddress = new Address(),
                NewPaymentMethod = new PaymentMethod()
            };

            return View("Index", model);
        }
    }
}
