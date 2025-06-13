// Product.cs (Model)
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public string? ImagePath { get; set; }

        public int? CategoryId { get; set; } // Foreign Key
        public Category? Category { get; set; } // Navigation Property

        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }
}


