using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using EcommerceMVC.Models;
using EcommerceMVC.Services;
using EcommerceMVC.Data;
using System;

namespace EcommerceMVC.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;  // Use interface here

        public CartController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            return View(cart?.CartItems ?? new List<CartItem>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(int productId, int quantity = 1)
        {
            if (quantity <= 0) quantity = 1;

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var product = await _context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId, LastModified = DateTime.UtcNow };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync(); // Save to generate CartId for FK
            }

            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                cartItem = new CartItem
                {
                    CartId = cart.CartId,
                    ProductId = productId,
                    Quantity = quantity
                };
                _context.CartItems.Add(cartItem);
            }

            cart.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int cartItemId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem == null || cartItem.Cart == null || cartItem.Cart.UserId != userId)
                return Forbid();

            if (quantity <= 0)
            {
                _context.CartItems.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = quantity;
            }

            cartItem.Cart.LastModified = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItem = await _context.CartItems
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

            if (cartItem == null || cartItem.Cart == null || cartItem.Cart.UserId != userId)
                return Forbid();

            cartItem.Cart.LastModified = DateTime.UtcNow;
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Admin: Send reminders for abandoned carts
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendAbandonmentReminders()
        {
            var abandonedCarts = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Include(c => c.User)
                .Where(c => c.CartItems.Any() && c.LastModified < DateTime.UtcNow.AddHours(-24))
                .ToListAsync();

            int emailsSent = 0;

            foreach (var cart in abandonedCarts)
            {
                if (cart.User != null && !string.IsNullOrEmpty(cart.User.Email))
                {
                    var cartItems = cart.CartItems.Select(ci => (ci.Product.Name, ci.Product.Price)).ToList();

                    await _emailService.SendCartAbandonmentReminderAsync(
                        cart.User.Email,
                        cart.User.UserName ?? "Valued Customer",
                        cartItems
                    );

                    cart.LastReminderSent = DateTime.UtcNow;
                    emailsSent++;
                }
            }

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = $"Sent {emailsSent} cart abandonment reminder emails." });
        }
    }
}
