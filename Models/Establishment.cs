using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delivery.Models;

public class Establishment
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    public string? Image { get; set; }
    
    [Required]
    public string Description { get; set; }

    public ICollection<Dish>? Dishes { get; set; }
    
    [NotMapped]
    public IFormFile? ImageFile { get; set; }
}