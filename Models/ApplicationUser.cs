using Microsoft.AspNetCore.Identity;

namespace EcommerceMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsAdmin { get; set; }

        public string? FullName { get; set; }
        
        // Flag to indicate if the user has opted in to promotional emails
        public bool PromotionalEmailsEnabled { get; set; } = false;
    }
}
