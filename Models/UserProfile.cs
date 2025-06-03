namespace EcommerceMVC.Models
{
    public class UserProfile
    {
        public int Id { get; set; }  // Optional primary key, especially if using EF Core
        public string UserId { get; set; } // Foreign key to Identity user

        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

        // Optional: Navigation property to ApplicationUser
        public ApplicationUser User { get; set; }
    }
}
