using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceMVC.Models
{
    public class Product
    {
        public int ProductId { get; set; }
        
        [Required(ErrorMessage = "Product name is required")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }
        
        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }
        
        [Required(ErrorMessage = "Stock quantity is required")]
        [Range(0, int.MaxValue, ErrorMessage = "Stock quantity must be 0 or greater")]
        [Display(Name = "Stock Quantity")]
        public int StockQuantity { get; set; }
        
        [Display(Name = "Image")]
        public string ImagePath { get; set; }
        
        [NotMapped] // This tells EF Core not to try to map this to a database column
        [Display(Name = "Upload Image")]
        public IFormFile ImageFile { get; set; }
    }
}