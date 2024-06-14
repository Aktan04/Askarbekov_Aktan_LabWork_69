namespace Delivery.Models;

public class CartItem
{
    public int Id { get; set; }
    
    public int CartId { get; set; }
    public Cart? Cart { get; set; }

    
    public int DishId { get; set; }
    public Dish? Dish { get; set; }

    public int Quantity { get; set; }
}