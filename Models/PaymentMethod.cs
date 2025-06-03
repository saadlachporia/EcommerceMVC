namespace EcommerceMVC.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Foreign key to Identity user

        public string CardHolderName { get; set; }
        public string CardNumber { get; set; }
        public string ExpiryMonth { get; set; }
        public string ExpiryYear { get; set; }
        public string CardType { get; set; }

        // Optional: Navigation property to ApplicationUser
        public ApplicationUser User { get; set; }
    }
}
