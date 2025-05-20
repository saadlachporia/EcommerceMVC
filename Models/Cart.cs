using EcommerceMVC.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
public class Cart
{
    public int CartId { get; set; }  // Primary Key

    public string UserId { get; set; }

    public ApplicationUser User { get; set; }


    public List<CartItem> CartItems { get; set; } = new List<CartItem>();

        // Navigation property for user
        
        // Timestamp for when the cart was last modified
        public DateTime LastModified { get; set; } = DateTime.UtcNow;
        
        // Timestamp for when the last reminder was sent (for abandoned carts)
        public DateTime? LastReminderSent { get; set; }
}
