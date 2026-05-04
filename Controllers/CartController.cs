using LaptopCart.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace LaptopCart.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cartItems = _context.CartItems.Where(c => c.UserId == userId).Include(X => X.Product).ToList();
            return View(cartItems);
        }
        public async Task<IActionResult> Plus(int cartId)
        {
            var cartFromDb = _context.CartItems.FirstOrDefault(c => c.Id == cartId);
            if (cartFromDb == null)
            {
                return NotFound();
            }

            cartFromDb.Quantity++;
            _context.CartItems.Update(cartFromDb);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult>Minus(int cartId)
        {
            var cartFromDb = _context.CartItems.FirstOrDefault(c => c.Id == cartId);
            if (cartFromDb == null)
            {
                return NotFound();
            }
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (cartFromDb.Quantity <= 1)
            {
                _context.CartItems.Remove(cartFromDb);
                await _context.SaveChangesAsync();

                var count = _context.CartItems.Count(c => c.UserId == userId);
                HttpContext.Session.SetInt32("CartCount", count);

            }
            else
            {
                cartFromDb.Quantity-= 1;
                _context.CartItems.Update(cartFromDb);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Remove(int cartId)
        {
            var cartFromDb = _context.CartItems.FirstOrDefault(c => c.Id == cartId);
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (cartFromDb == null)
            {
                return NotFound();
            }

            _context.CartItems.Remove(cartFromDb);
            HttpContext.Session.SetInt32("CartCount", _context.CartItems.Count(c => c.UserId == userId) - 1);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }

    }
