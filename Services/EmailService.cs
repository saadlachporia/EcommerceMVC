using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace EcommerceMVC.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            await SendEmailAsync(toEmail, subject, body, isHtml: false);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body, bool isHtml = false)
        {
            try
            {
                var settings = _configuration.GetSection("SmtpSettings");
                var host = settings["Host"];
                var portStr = settings["Port"];
                var username = settings["UserName"];
                var password = settings["Password"];
                var enableSslStr = settings["EnableSsl"];
                var fromEmail = settings["FromEmail"] ?? username;

                if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(portStr) ||
                    string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                {
                    _logger.LogWarning("SMTP settings are not properly configured.");
                    return;
                }

                if (!int.TryParse(portStr, out var port))
                {
                    _logger.LogWarning("SMTP Port is invalid.");
                    return;
                }

                if (!bool.TryParse(enableSslStr, out var enableSsl))
                {
                    enableSsl = true;
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Ecommerce Store", fromEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;
                message.Body = isHtml
                    ? new TextPart("html") { Text = body }
                    : new TextPart("plain") { Text = body };

                using var smtp = new SmtpClient();
                var secureSocketOption = enableSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
                if (port == 465)
                    secureSocketOption = SecureSocketOptions.SslOnConnect;

                await smtp.ConnectAsync(host, port, secureSocketOption);
                await smtp.AuthenticateAsync(username, password);
                await smtp.SendAsync(message);
                await smtp.DisconnectAsync(true);

                _logger.LogInformation("Email sent successfully to {Email}", toEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }

        public async Task SendOrderConfirmationAsync(string email, string customerName, int orderId, decimal total, IEnumerable<(string productName, int quantity, decimal price)> items)
        {
            var subject = $"Order Confirmation #{orderId} - Thank You for Your Purchase!";
            var body = $@"Dear {customerName},

Thank you for your order! We're pleased to confirm that we've received your order #{orderId}.

Order Summary:
";

            foreach (var item in items)
            {
                body += $"- {item.productName} x {item.quantity} = ${item.price * item.quantity:F2}\n";
            }

            body += $@"
Total: ${total:F2}

You can track your order status by visiting our website and entering your order number.

Thank you for shopping with us!

Best regards,
The Ecommerce Store Team";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendShippingUpdateAsync(string email, string customerName, int orderId, string status, string? trackingNumber = null)
        {
            var subject = $"Shipping Update for Order #{orderId}";
            var body = $@"Dear {customerName},

We have an update regarding your order #{orderId}.

Your order status has been updated to: {status}";

            if (!string.IsNullOrEmpty(trackingNumber))
            {
                body += $@"

Your shipment is on its way! Your tracking number is: {trackingNumber}
You can use this number to track your package on our website or the carrier's website.";
            }

            body += @"

Thank you for your patience and for choosing our store!

Best regards,
The Ecommerce Store Team";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendAccountCreationConfirmationAsync(string email, string username)
        {
            var subject = "Welcome to Our Store - Account Created Successfully";
            var body = $@"Hello {username},

Welcome to our online store! Your account has been successfully created.

You can now:
- Browse our extensive product catalog
- Save items to your wishlist
- Track your orders
- Receive exclusive offers and promotions

Thank you for joining us!

Best regards,
The Ecommerce Store Team";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetAsync(string email, string resetLink)
        {
            var subject = "Password Reset Request";
            var body = $@"Hello,

We received a request to reset your password for your account. If you didn't make this request, you can safely ignore this email.

To reset your password, please click the link below:
{resetLink}

This link will expire in 24 hours.

Best regards,
The Ecommerce Store Team";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPromotionalEmailAsync(string email, string customerName, string promotionTitle, string promotionDetails, string? couponCode = null)
        {
            var subject = $"Special Offer: {promotionTitle}";
            var body = $@"Hello {customerName},

We're excited to share this special offer with you!

{promotionTitle}
{promotionDetails}";

            if (!string.IsNullOrEmpty(couponCode))
            {
                body += $@"

Use this coupon code at checkout: {couponCode}";
            }

            body += @"

Visit our store to take advantage of this limited-time offer.

If you wish to unsubscribe from promotional emails, please visit your account settings.

Best regards,
The Ecommerce Store Team";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendCartAbandonmentReminderAsync(string email, string customerName, List<(string productName, decimal productPrice)> items)
        {
            var subject = "Your Items Are Still Waiting for You";
            var body = $@"Hello {customerName},

We noticed you left some items in your shopping cart. Don't worry, we've saved them for you!

Items in your cart:";

            foreach (var item in items)
            {
                body += $"\n- {item.productName} - ${item.productPrice:F2}";
            }

            body += @"

To complete your purchase, simply return to our website and proceed to checkout.

Thank you for considering our products!

Best regards,
The Ecommerce Store Team";

            await SendEmailAsync(email, subject, body);
        }
    }
}
