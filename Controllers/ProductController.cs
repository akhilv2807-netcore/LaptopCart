using LaptopCart.Data;
using LaptopCart.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LaptopCart.Controllers
{
  
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ProductController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId != null)
            {
                HttpContext.Session.SetInt32(SD.SessionCart, _context.CartItems.Where(c => c.UserId == userId).Count());
            }
            var products = _context.Products.ToList();
            return View(products);  
        }
        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p=>p.Id==id);
            return View(product);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Details(CartItem cartItem)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartProduct = _context.CartItems.FirstOrDefault(p=> p.ProductId == cartItem.ProductId && p.UserId == userId);
            if (cartProduct != null)
            {
                cartProduct.Quantity += cartItem.Quantity;
                _context.CartItems.Update(cartProduct);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            else
            {
                cartItem.Id = 0; // Ensure a new CartItem is created
                cartItem.UserId = userId;
                _context.CartItems.Add(cartItem);
                await _context.SaveChangesAsync();
            }
                return RedirectToAction("Index");
            
        }
        }
}
