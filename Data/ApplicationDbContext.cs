// Models/ApplicationDbContext.cs
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace EcommerceMVC.Models
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        // Add your DbSets for other entities here
        // public DbSet<Product> Products { get; set; }
        // public DbSet<Category> Categories { get; set; }
        // public DbSet<Order> Orders { get; set; }
    }
}