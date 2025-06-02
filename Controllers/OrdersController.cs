using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using EcommerceMVC.Models;
using EcommerceMVC.Models.OrderViewModels;
using EcommerceMVC.Services;

namespace EcommerceMVC.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrdersController> _logger;
        private readonly IEmailService _emailService;

        public OrdersController(
            ApplicationDbContext context,
            ILogger<OrdersController> logger,
            IEmailService emailService)
        {
            _context = context;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<IActionResult> Index(string status = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId && (status == null || o.Status == status))
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            return order == null ? NotFound() : View(order);
        }

        public async Task<IActionResult> Confirmation(int? id)
        {
            if (id == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId);

            return order == null ? NotFound() : View(order);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateStatus(int orderId, string status, string trackingNumber = null)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null) return NotFound();

            order.Status = status;
            if (!string.IsNullOrEmpty(trackingNumber))
            {
                order.TrackingNumber = trackingNumber;
            }

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(order.Email))
            {
                try
                {
                    await _emailService.SendShippingUpdateAsync(
                        order.Email,
                        order.FullName,
                        orderId,
                        status,
                        trackingNumber
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send shipping update email for OrderId {OrderId}", orderId);
                }
            }

            return Json(new { success = true, message = "Status updated and shipping notification email sent." });
        }

        public async Task<IActionResult> Checkout()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
                return RedirectToAction("Index", "Cart");

            var model = new CheckoutViewModel
            {
                CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    ProductPrice = ci.Product.Price
                }).ToList(),
                Total = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity)
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                ModelState.AddModelError("", "Your cart is empty.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                model.CartItems = cart.CartItems.Select(ci => new CartItemViewModel
                {
                    ProductName = ci.Product.Name,
                    Quantity = ci.Quantity,
                    ProductPrice = ci.Product.Price
                }).ToList();
                model.Total = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity);
                return View(model);
            }

            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                TotalAmount = cart.CartItems.Sum(ci => ci.Product.Price * ci.Quantity),
                FullName = model.FullName,
                Address = model.Address,
                Email = model.Email,
                Phone = model.Phone,
                PaymentMethod = model.PaymentMethod,
                Status = "Pending",
                OrderItems = cart.CartItems.Select(ci => new OrderItem
                {
                    ProductId = ci.ProductId,
                    Quantity = ci.Quantity,
                    Price = ci.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.CartItems.RemoveRange(cart.CartItems);
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(order.Email))
            {
                try
                {
                    var orderItems = order.OrderItems.Select(item =>
                        (item.Product.Name, item.Quantity, item.Price)).ToList();

                    await _emailService.SendOrderConfirmationAsync(
                        order.Email,
                        order.FullName,
                        order.OrderId,
                        order.TotalAmount,
                        orderItems
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send order confirmation email for OrderId {OrderId}", order.OrderId);
                }
            }

            return RedirectToAction("Confirmation", new { id = order.OrderId });
        }

        public IActionResult TrackOrder() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TrackOrder(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null)
            {
                ModelState.AddModelError("", "Order not found. Please check your Order ID.");
                return View();
            }

            var viewModel = new TrackOrderViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                FullName = order.FullName,
                Address = order.Address,
                Status = order.Status,
                TotalAmount = order.TotalAmount,
                TrackingNumber = order.TrackingNumber,
                Items = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductName = oi.Product.Name,
                    Quantity = oi.Quantity,
                    Price = oi.Price
                }).ToList()
            };

            return View("TrackOrderResult", viewModel);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendPromotionalEmail(string promotionTitle, string promotionDetails, string couponCode = null)
        {
            var subscribers = await _context.Users
                .Where(u => u.PromotionalEmailsEnabled)
                .ToListAsync();

            int emailsSent = 0;

            foreach (var user in subscribers)
            {
                if (!string.IsNullOrEmpty(user.Email))
                {
                    try
                    {
                        await _emailService.SendPromotionalEmailAsync(
                            user.Email,
                            user.UserName ?? "Valued Customer",
                            promotionTitle,
                            promotionDetails,
                            couponCode
                        );
                        emailsSent++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to send promotional email to {Email}", user.Email);
                    }
                }
            }

            return Json(new { success = true, message = $"Sent promotional email to {emailsSent} subscribers." });
        }
    }
}
