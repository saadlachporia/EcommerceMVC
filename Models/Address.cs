namespace EcommerceMVC.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Foreign key to Identity user

        public string Line1 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }

        // Optional: Navigation property to ApplicationUser
        public ApplicationUser User { get; set; }
    }
}
