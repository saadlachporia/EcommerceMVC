using EcommerceMVC.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.ViewModels
{
    public class ProfileViewModel
    {
        public UserProfile Profile { get; set; }
        public List<Address> Addresses { get; set; }
        public Address NewAddress { get; set; } = new Address();
        public List<PaymentMethod> PaymentMethods { get; set; }
        public PaymentMethod NewPaymentMethod { get; set; } = new PaymentMethod();
        public ChangePasswordViewModel PasswordModel { get; set; } = new ChangePasswordViewModel();
    }

        public class PasswordViewModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }


    public class PasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        public string CurrentPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
