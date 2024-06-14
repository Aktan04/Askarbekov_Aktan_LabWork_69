using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers;

public class CartController : BaseController
{
    private readonly DeliveryContext _context;
    private readonly UserManager<User> _userManager;

    public CartController(DeliveryContext context, UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> AddToCart(int dishId)
    {
        var dish = await _context.Dishes.FindAsync(dishId);
        if (dish == null)
        {
            return NotFound();
        }
        
        Cart cart = new Cart { UserId = Convert.ToInt32(CurrentUserId), EstablishmentId = dish.EstablishmentId, DishId = dishId };
        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetCart(int? establishmentId)
    {
        try
        {
            var user = await _userManager.GetUserAsync(User);
            var carts = await _context.Carts
                .Include(e => e.Establishment )
                .Include(c => c.Dish )
                .Where(c => c.UserId == user.Id && c.Dish.EstablishmentId == establishmentId)
                .ToListAsync();

            return PartialView("_CartPartial", carts);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка в методе GetCart: {ex.Message}");
            return StatusCode(500, "Ошибка при получении корзины");
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> RemoveFromCart(int cartId)
    {
        var cart = await _context.Carts.FindAsync(cartId);
        if (cart != null)
        {
            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();
        }
        return Ok();
    }
    
}