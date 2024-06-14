namespace Delivery.Models;

public class Cart
{
    public int Id { get; set; }

    
    public int UserId { get; set; }
    public User? User { get; set; }

    public int EstablishmentId { get; set; }
    public Establishment? Establishment { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; }
}