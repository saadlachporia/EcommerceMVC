using EcommerceMVC.Models;
namespace EcommerceMVC.Models
{
    public class CartItem
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }

        // public string ProductName { get; set; }
        public decimal Price { get; set; }

        public Product Product { get; set; } = null!;
        public Cart Cart { get; set; } = null!;
    }
}



