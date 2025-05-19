using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Models.OrderViewModels
{
    public class CheckoutViewModel
    {
        public int OrderId { get; set; }          // Add this
        public DateTime OrderDate { get; set; }  // And this

        [Required(ErrorMessage = "Full Name is required.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Phone(ErrorMessage = "Invalid Phone Number.")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; }

        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        public decimal Total { get; set; }
    }

    public class OrderItemViewModel
    {
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
    }

    // You can also define CartItemViewModel here or elsewhere as needed:

}
