// Models/Category.cs
using System.ComponentModel.DataAnnotations;

namespace EcommerceMVC.Models
{
    public class Category
    {
        public int? CategoryId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}
