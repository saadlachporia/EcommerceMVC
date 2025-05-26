// using System.ComponentModel.DataAnnotations;

// namespace EcommerceMVC.Models
// {
//     public class RegisterViewModel
//     {
//         [Required]
//         [EmailAddress]
//         [Display(Name = "Email")]
//         public string Email { get; set; }

//         [Required]
//         [DataType(DataType.Password)]
//         [Display(Name = "Password")]
//         public string Password { get; set; }

//         [DataType(DataType.Password)]
//         [Display(Name = "Confirm password")]
//         [Compare("Password", ErrorMessage = "Passwords do not match.")]
//         public string ConfirmPassword { get; set; }
//     }
// }

using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Receive promotional emails")]
        public bool PromotionalEmailsEnabled { get; set; } = true;
    }

    public class ManageAccountViewModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Receive promotional emails")]
        public bool PromotionalEmailsEnabled { get; set; }
    }
}
