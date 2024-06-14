using Delivery.Models;
using Delivery.Services;
using Delivery.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace Delivery.Controllers;

public class AccountController : Controller
{
    public DeliveryContext _context;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IWebHostEnvironment _hostEnvironment;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager,
        IWebHostEnvironment hostEnvironment, DeliveryContext context)
    {
        _context = context;
        _userManager = userManager;
        _signInManager = signInManager;
        _hostEnvironment = hostEnvironment;
    }
    
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Index()
    {
        if (User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("admin"))
            {
                var users = await _context.Users.Where(u => u.Id > 1).ToListAsync();
                var userRoles = new Dictionary<int, IList<string>>();
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userRoles[user.Id] = roles;
                }
                ViewBag.UserRoles = userRoles;
                return View(users);
            }
        }

        return NotFound();
    }
    
    [Authorize]
    public async Task<IActionResult> Profile(int? userId)
    {
        if (userId != null)
        {
            var getUser = _context.Users.FirstOrDefault(u => u.Id == userId);
            return View(getUser);
        }
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }

        return View(user);
    }

    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddToManager(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }
        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        var resultOfRemove = await _userManager.RemoveFromRoleAsync(identityUser, "user");
        if (!resultOfRemove.Succeeded)
        {
            return BadRequest("Не удалось удалить роль пользователя");
        }
        var result = await _userManager.AddToRoleAsync(identityUser, "manager");
        if (!result.Succeeded)
        {
            return BadRequest("Не удалось добавить роль менеджера");
        }
        
        return RedirectToAction("Index", "Account");
    }
    
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RemoveFromManager(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user == null)
        {
            return NotFound("Пользователь не найден");
        }
        var identityUser = await _userManager.FindByEmailAsync(user.Email);
        var resultOfRemove = await _userManager.RemoveFromRoleAsync(identityUser, "manager");
        if (!resultOfRemove.Succeeded)
        {
            return BadRequest("Не удалось удалить роль менеджера");
        }
        var result = await _userManager.AddToRoleAsync(identityUser, "user");
        if (!result.Succeeded)
        {
            return BadRequest("Не удалось добавить роль пользователя");
        }
        
        return RedirectToAction("Index", "Account");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(int id)
    {
        int userId = Convert.ToInt32(_userManager.GetUserId(User));
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user != null && (userId == id || User.IsInRole("admin")))
        {
            return View(user);
        }

        return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Edit(User user)
    {
        if (_context.Users.Any(u => u.Email == user.Email && u.Id != user.Id) || 
            _context.Users.Any(u => u.UserName == user.UserName && u.Id != user.Id))
        {
            ModelState.AddModelError(string.Empty, "Логин или Email уже существует");
            return View(user);
        }
        string? userId = _userManager.GetUserId(User);
        User identityUser = await _userManager.FindByIdAsync(Convert.ToString(user.Id));
        if (identityUser != null)
        {
            if (ModelState.IsValid)
            {
                if (user.ImageFile != null && user.ImageFile.Length > 0)
                {
                    var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(user.ImageFile.FileName);
                    var fullPath = Path.Combine(uploadPath, fileName);

                    using (var fileStream = new FileStream(fullPath, FileMode.Create))
                    {
                        await user.ImageFile.CopyToAsync(fileStream);
                    }

                    identityUser.Avatar = "/images/" + fileName;
                }

                identityUser.UserName = user.UserName;
                identityUser.NickName = user.NickName;
                identityUser.Email = user.Email;
                var result = await _userManager.UpdateAsync(identityUser);
                if (result.Succeeded)
                {
                    return RedirectToAction("Profile");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        return View(user);
    }
    
    [Authorize]
    [HttpGet]
    [ActionName("Delete")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ConfirmDelete(int? id)
    {
        if (id != null)
        {
            User? user = await _context.Users.FirstOrDefaultAsync(p => p.Id == id);
            if (user != null && User.IsInRole("admin"))
            {
                return View(user);
            }
        }
        return NotFound();
    }
    
    [HttpPost]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id != null)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }
        return NotFound();
    }
    
    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
       return View(new LoginViewModel(){ReturnUrl = returnUrl});
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            User? user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                user = await _userManager.FindByNameAsync(model.Email);
            }
            if (user != null)
            {
                
                SignInResult result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction("Index", "Home");// change redirect
                }
            }
            ModelState.AddModelError("", "Invalid email or password");
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        User? findUserWithEmail = _context.Users.FirstOrDefault(u => u.Email == model.Email);
        User? findUserWithUserName = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
        if (findUserWithEmail != null || findUserWithUserName != null)
        {
            ModelState.AddModelError("UserName", "Логин или Email уже существует");
            return View(model);
        }
        model.Avatar = "/images/default.png";
        if (ModelState.IsValid)
        {
            if (model.ImageFile != null && model.ImageFile.Length > 0)
            {
                
                var uploadPath = Path.Combine(_hostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ImageFile.FileName);
                var fullPath = Path.Combine(uploadPath, fileName);
            
                using (var fileStream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.ImageFile.CopyToAsync(fileStream);
                }
            
                model.Avatar = "/images/" + fileName;
            }
            
            User user = new User()
            {
                Email = model.Email,
                UserName = model.UserName,
                Avatar = model.Avatar,
                NickName = model.NickName,
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "user");
                if (!User.IsInRole("admin"))
                {
                    await _signInManager.SignInAsync(user, false);
                }
                return RedirectToAction("Index", "Home");// change redirect 
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Login", "Account");
    }

    public async Task<IActionResult> AccessDenied(string returnUrl = null)
    {
        return RedirectToAction("Login", new { returnUrl = returnUrl });
    }
}