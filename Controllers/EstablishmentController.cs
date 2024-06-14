using Delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Controllers;

public class EstablishmentController : Controller
{
    private readonly DeliveryContext _context;
    private readonly IWebHostEnvironment _hostEnvironment;

    public EstablishmentController(DeliveryContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }
    
    public async Task<IActionResult> Index()
    {
        var establishments = await _context.Establishments.ToListAsync();
        return View(establishments);
    }

    public async Task<IActionResult> Details(int id)
    {
        var establishment = await _context.Establishments.Include(e => e.Dishes)
                .FirstOrDefaultAsync(e => e.Id == id);
        if (establishment == null)
        {
                return NotFound();
        }
        return View(establishment);
    }

    [Authorize(Roles = "admin, manager")]
    public IActionResult Create()
    {
        return View();
    }

    [Authorize(Roles = "admin, manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Establishment establishment)
    {
        if (establishment.ImageFile == null)
        {
            ModelState.AddModelError("ImageFile", "Картинка обязательна для скачивания");
            return View(establishment);
        }

        if (await _context.Establishments.FirstOrDefaultAsync(e => e.Name == establishment.Name) != null)
        {
            ModelState.AddModelError("Name", "Заведение с таким именем уже существует");
            return View(establishment);
        }
        if (ModelState.IsValid)
        {
            if (establishment.ImageFile != null && establishment.ImageFile.Length > 0)
            {
                
                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(establishment.ImageFile.FileName);
                var fullPath = Path.Combine(uploadPath, fileName);
            
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await establishment.ImageFile.CopyToAsync(fileStream);
                }
            
                establishment.Image = "/images/" + fileName;
            }
            _context.Add(establishment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(establishment);
    }

    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var establishment = await _context.Establishments.Include(e => e.Dishes)
            .FirstOrDefaultAsync(e => e.Id == id);
        if (establishment == null)
        {
            return NotFound();
        }
        return View(establishment);
    }

    [Authorize(Roles = "admin, manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Establishment establishment)
    {
        if (await _context.Establishments.FirstOrDefaultAsync(e => e.Name == establishment.Name && e.Id != establishment.Id) != null)
        {
            establishment.Dishes = await _context.Dishes.Where(d => d.EstablishmentId == establishment.Id).ToListAsync();

            ModelState.AddModelError("Name", "Заведение с таким именем уже существует");
            return View(establishment);
        }
        if (ModelState.IsValid)
        {
            if (establishment.ImageFile != null && establishment.ImageFile.Length > 0)
            {
                
                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadPath))
                { 
                    Directory.CreateDirectory(uploadPath);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(establishment.ImageFile.FileName);
                var fullPath = Path.Combine(uploadPath, fileName);
            
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await establishment.ImageFile.CopyToAsync(fileStream);
                }
            
                establishment.Image = "/images/" + fileName;
            }
            _context.Update(establishment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(establishment);
    }

    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var establishment = await _context.Establishments.FindAsync(id);
        if (establishment != null)
        {
            _context.Establishments.Remove(establishment);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> CreateDish(int establishmentId)
    {
        Establishment? establishment = await _context.Establishments.FirstOrDefaultAsync(e => e.Id == establishmentId);
        if (establishment != null)
        {
            return View(new Dish() {Establishment = establishment});
        }

        return NotFound();
    }

    [Authorize(Roles = "admin, manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateDish(Dish dish)
    {
        if (ModelState.IsValid)
        {
            _context.Add(dish);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = dish.EstablishmentId });
        }
        return View(dish);
    }

    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> EditDish(int id)
    {
        var dish = await _context.Dishes.FindAsync(id);
        if (dish == null)
        {
            return NotFound();
        }
        return View(dish);
    }

    [Authorize(Roles = "admin, manager")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditDish(Dish dish)
    {
        if (ModelState.IsValid)
        { 
            _context.Update(dish); 
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { id = dish.EstablishmentId });
        }
        return View(dish);
    }

    [Authorize(Roles = "admin, manager")]
    public async Task<IActionResult> DeleteDish(int id)
    {
        var dish = await _context.Dishes.FindAsync(id);
        if (dish != null)
        {
            _context.Dishes.Remove(dish);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction("Details", new { id = dish.EstablishmentId });
    }
}