using EcommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMVC.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace EcommerceMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products.ToListAsync();
            return View(products);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View(new Product());
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Upload image BEFORE checking ModelState
            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                product.ImagePath = "/images/" + uniqueFileName;
            }

            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Debugging output
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("ModelState error: " + error.ErrorMessage);
            }

            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ProductId,Name,Description,Price,StockQuantity,ImagePath,ImageFile")] Product product)
        {
            if (id != product.ProductId)
                return NotFound();

            // Always clear the ImageFile validation error if we have an existing image path
            ModelState.Remove("ImageFile"); // This will remove the validation error for ImageFile completely
            
            // Retrieve the existing product to get the current ImagePath if none provided
            if (string.IsNullOrEmpty(product.ImagePath))
            {
                var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
                if (existingProduct != null)
                {
                    product.ImagePath = existingProduct.ImagePath;
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Handle the image upload if a new image is provided
                    if (product.ImageFile != null && product.ImageFile.Length > 0)
                    {
                        string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await product.ImageFile.CopyToAsync(stream);
                        }

                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(product.ImagePath))
                        {
                            string oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                try
                                {
                                    System.IO.File.Delete(oldImagePath);
                                }
                                catch (Exception ex)
                                {
                                    // Log the error but continue
                                    Console.WriteLine("Error deleting old image: " + ex.Message);
                                }
                            }
                        }

                        product.ImagePath = "/images/" + uniqueFileName;
                    }

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == product.ProductId))
                        return NotFound();
                    else
                        throw;
                }
            }

            // Debugging output
            Console.WriteLine("ModelState is invalid in Edit action");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("ModelState error: " + error.ErrorMessage);
            }

            return View(product);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products.FirstOrDefaultAsync(m => m.ProductId == id);
            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Optionally delete image file associated with the product
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", product.ImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        try
                        {
                            System.IO.File.Delete(imagePath); // Delete image if it exists
                        }
                        catch (Exception ex)
                        {
                            // Handle any exceptions that may occur during image deletion
                            Console.WriteLine("Error deleting image: " + ex.Message);
                        }
                    }
                }

                // Remove the product from the database
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Index");
        }
    }
}