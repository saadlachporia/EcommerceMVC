using Microsoft.AspNetCore.Identity;

namespace EcommerceMVC.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsAdmin { get; set; }
    }
}
