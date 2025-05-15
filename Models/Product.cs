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
    public string Name { get; set; }
    
    public string Description { get; set; }
    
    [Required]
    public decimal Price { get; set; }
    
    public int StockQuantity { get; set; }
    
    public string? ImagePath { get; set; }

    [NotMapped]
    public IFormFile? ImageFile { get; set; }
}
}