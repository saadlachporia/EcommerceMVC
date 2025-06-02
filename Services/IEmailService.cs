using System.Collections.Generic;
using System.Threading.Tasks;

namespace EcommerceMVC.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendAccountCreationConfirmationAsync(string toEmail, string userName);
        Task SendPasswordResetAsync(string toEmail, string resetLink);
        Task SendCartAbandonmentReminderAsync(string toEmail, string userName, List<(string productName, decimal productPrice)> cartItems);

        // ✅ Add these missing declarations:
        Task SendOrderConfirmationAsync(string email, string customerName, int orderId, decimal total, IEnumerable<(string productName, int quantity, decimal price)> items);
        Task SendShippingUpdateAsync(string email, string customerName, int orderId, string status, string? trackingNumber = null);
        Task SendPromotionalEmailAsync(string email, string customerName, string promotionTitle, string promotionDetails, string? couponCode = null);
    }
}
