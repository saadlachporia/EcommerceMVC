using EcommerceMVC.Models;
public class Cart
{
    public int CartId { get; set; }  // Primary Key

    public string UserId { get; set; }

    public List<CartItem> CartItems { get; set; } = new List<CartItem>();
}
