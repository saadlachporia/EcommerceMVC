using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using EcommerceMVC.Models;
using EcommerceMVC.Settings;
using Microsoft.Extensions.Logging;

namespace EcommerceMVC.Controllers
{
    public class ContactController : Controller
    {
        private readonly SmtpSettings _smtp;
        private readonly ILogger<ContactController> _log;

        public ContactController(IOptions<SmtpSettings> smtp, ILogger<ContactController> log)
        {
            _smtp = smtp.Value;
            _log  = log;
        }

        // Show empty form
        [HttpGet]
        public IActionResult Index() => View(new Contact());

        // Handle form POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(Contact model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                // Build the email
                var mail = new MailMessage
                {
                    From       = new MailAddress(_smtp.FromEmail, "Website Enquiries"),
                    Subject    = $"[Contact] {model.Name}",
                    Body       = $"Name: {model.Name}\nEmail: {model.Email}\nPhone: {model.Phone}\nOrder ID: {model.OrderID}\n\n{model.Message}",
                    IsBodyHtml = false
                };
                mail.To.Add("saadlachporia25@gmail.com");      // destination
                mail.ReplyToList.Add(model.Email);            // lets you click “Reply” to answer the sender

                // Send it
                using var client = new SmtpClient(_smtp.Host, _smtp.Port)
                {
                    EnableSsl            = _smtp.EnableSsl,
                    Credentials          = new NetworkCredential(_smtp.UserName, _smtp.Password),
                    DeliveryMethod       = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                await client.SendMailAsync(mail);

                ViewBag.Success = "Thank you! Your message has been delivered.";
                ModelState.Clear();                     // resets the form
                return View(new Contact());             // fresh form
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Contact form email failed");
                ViewBag.Error = "Sorry, something went wrong while sending your message.";
                return View(model);
            }
        }
    }
}
