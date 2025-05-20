using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        public string UserId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalAmount { get; set; }

        // Customer information added to Order
        [Required]
        public string FullName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, Phone]
        public string Phone { get; set; }

        [Required]
        public string PaymentMethod { get; set; }

        // Navigation property for order items
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        public string Status { get; set; }

        // Tracking number for shipment
        public string? TrackingNumber { get; set; }
    }
}
