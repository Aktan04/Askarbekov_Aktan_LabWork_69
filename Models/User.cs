using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Delivery.Services;
using Microsoft.AspNetCore.Identity;

namespace Delivery.Models;

public class User : IdentityUser<int>
{
    public string? Avatar { get; set; }
    [Required]
    public string NickName { get; set; }
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
    public IdentityRole? Role { get; set; }
}