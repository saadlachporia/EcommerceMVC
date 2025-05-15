using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceMVC.Data;
using EcommerceMVC.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace EcommerceMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)   // Eager load Category
                .ToListAsync();

            return View(products);
        }

        // GET: Products/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name");
            return View();
        }

        // POST: Products/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product)
        {
            if (product.ImageFile == null || product.ImageFile.Length == 0)
            {
                ModelState.AddModelError("ImageFile", "Please upload an image.");
            }

            if (ModelState.IsValid)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
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

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // If we got here, re-populate categories for the dropdown
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)  // Load category for display if needed
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Products/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
                return NotFound();

            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductId == id);
            if (existingProduct == null)
                return NotFound();

            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
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
            else
            {
                product.ImagePath = existingProduct.ImagePath;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == product.ProductId))
                        return NotFound();
                    else
                        throw;
                }

                return RedirectToAction(nameof(Index));
            }

            // Re-populate categories if model validation failed
            ViewBag.Categories = new SelectList(_context.Categories, "CategoryId", "Name", product.CategoryId);
            return View(product);
        }

        // GET: Products/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var product = await _context.Products
                .Include(p => p.Category)  // Include category for confirmation display
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
