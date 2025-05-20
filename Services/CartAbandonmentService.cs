using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using EcommerceMVC.Models;
using EcommerceMVC.Services;

namespace EcommerceMVC.Services
{
    public class CartAbandonmentService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<CartAbandonmentService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromHours(24); // Check once a day

        public CartAbandonmentService(
            IServiceProvider services,
            ILogger<CartAbandonmentService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Cart Abandonment Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckForAbandonedCarts();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking for abandoned carts.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }

            _logger.LogInformation("Cart Abandonment Service is stopping.");
        }

        private async Task CheckForAbandonedCarts()
        {
            _logger.LogInformation("Checking for abandoned carts...");

            using var scope = _services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            // Find all carts that:
            // 1. Have items in them
            // 2. Haven't been modified in the last 24 hours
            // 3. Either have never had a reminder sent OR the last reminder was sent more than 7 days ago
            // (to avoid spamming users)
            var now = DateTime.UtcNow;
            var abandonedCarts = await dbContext.Carts
                .Include(c => c.CartItems)
                    .ThenInclude(ci => ci.Product)
                .Include(c => c.User)
                .Where(c => c.CartItems.Any() && 
                            c.LastModified < now.AddHours(-24) &&
                            (c.LastReminderSent == null || c.LastReminderSent < now.AddDays(-7)))
                .ToListAsync();

            int emailsSent = 0;

            foreach (var cart in abandonedCarts)
            {
                if (cart.User != null && !string.IsNullOrEmpty(cart.User.Email))
                {
                    // Only send reminder emails to users who have opted in to promotional emails
                    if (cart.User.PromotionalEmailsEnabled)
                    {
                        var cartItems = cart.CartItems.Select(ci => 
                            (ci.Product.Name, ci.Product.Price)).ToList();
                        
                        await emailService.SendCartAbandonmentReminderAsync(
                            cart.User.Email,
                            cart.User.FullName ?? cart.User.UserName ?? "Valued Customer",
                            cartItems
                        );
                        
                        emailsSent++;
                        
                        // Update the cart's last reminder sent timestamp
                        cart.LastReminderSent = now;
                    }
                }
            }

            if (emailsSent > 0)
            {
                await dbContext.SaveChangesAsync();
                _logger.LogInformation("Sent {Count} cart abandonment reminder emails", emailsSent);
            }
            else
            {
                _logger.LogInformation("No abandoned carts found requiring reminders");
            }
        }
    }
}