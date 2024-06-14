using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Delivery.Models;

public class Dish
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public int Price { get; set; }
    
    [Required]
    public string Description { get; set; }

    [Required]
    public int EstablishmentId { get; set; }
        
    public  Establishment? Establishment { get; set; }
}