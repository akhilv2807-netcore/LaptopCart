using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;

namespace LaptopCart.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public AdminProductController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {
            return View(_context.Products.ToList());
        }
        public IActionResult Create() => View();

        public async Task<IActionResult> Edit(int id)
        { 
            var product = await _context.Products.FindAsync(id);
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(Product product)
        {
            if (product == null)
            {
                return NotFound();
            }

            // Fetch the existing product to preserve the image path if no new image is uploaded
            var existingProduct = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existingProduct == null)
            {
                return NotFound();
            }

            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                // Clean and generate a unique filename
                string originalfileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName).Replace(" ", "_"); // Remove spaces
                string extension = Path.GetExtension(product.ImageFile.FileName);
                string uniqueFileName = $"{originalfileName}_{Guid.NewGuid():N}{extension}";
                // Ensure the /images folder exists
                string imagesFolder = Path.Combine(wwwRootPath, "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }
                // Path to save the image physically
                string filePath = Path.Combine(imagesFolder, uniqueFileName);
                // Save file to server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }
                // Save relative path (for Razor ‹img src=...›)
                product.ImagePath = "/images/" + uniqueFileName;

                // Delete old image if it exists
                if (!string.IsNullOrEmpty(existingProduct.ImagePath))
                {
                    string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, existingProduct.ImagePath.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(oldFilePath))
                        System.IO.File.Delete(oldFilePath);
                }

                //[Optional] Verify file was saved -useful for debugging
                string confirmPath = Path.Combine(wwwRootPath, product.ImagePath.TrimStart('/'));
                if (!System.IO.File.Exists(confirmPath))
                {
                    throw new FileNotFoundException("Image was not saved correctly", confirmPath);
                }
            }
            else
            {
                // Keep the existing image path if no new image is provided
                product.ImagePath = existingProduct.ImagePath;
            }

            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                return View(product);
            }

        }
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // If the product has an associated image, delete the image file from the server
                if (!string.IsNullOrEmpty(product.ImagePath))
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, product.ImagePath.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(filePath))
                        System.IO.File.Delete(filePath);
                }
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (product == null)
            {
                return BadRequest();
            }

            if (product.ImageFile != null && product.ImageFile.Length > 0)
            {
                // Get the wwwroot path from the environment
                string wwwRootPath = _webHostEnvironment.WebRootPath;
                // Clean and generate a unique filename
                string originalfileName = Path.GetFileNameWithoutExtension(product.ImageFile.FileName).Replace(" ", "_"); // Remove spaces
                string extension = Path.GetExtension(product.ImageFile.FileName);
                string uniqueFileName = $"{originalfileName}_{Guid.NewGuid():N}{extension}";
                // Ensure the /images folder exists
                string imagesFolder = Path.Combine(wwwRootPath, "images");
                if (!Directory.Exists(imagesFolder))
                {
                    Directory.CreateDirectory(imagesFolder);
                }
                // Path to save the image physically
                string filePath = Path.Combine(imagesFolder, uniqueFileName);
                // Save file to server
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }
                // Save relative path (for Razor ‹img src=...›)
                product.ImagePath = "/images/" + uniqueFileName;
                //[Optional] Verify file was saved -useful for debugging
                string confirmPath = Path.Combine(wwwRootPath, product.ImagePath.TrimStart('/'));
                if (!System.IO.File.Exists(confirmPath))
                {
                    throw new FileNotFoundException("Image was not saved correctly", confirmPath);
                }
            }

            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                return View(product);
            }
        }
    }
}
