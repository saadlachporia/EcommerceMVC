// Models/AccountViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Models
{
public class AccountViewModel
{
    // Login fields
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public bool RememberMe { get; set; }

    // Register fields
    [Required]
    [EmailAddress]
    public string RegisterEmail { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string RegisterPassword { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("RegisterPassword")]
    public string ConfirmPassword { get; set; }
}

}
