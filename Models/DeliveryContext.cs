using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Models;

public class DeliveryContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DeliveryContext(DbContextOptions<DeliveryContext> options) : base(options){}

    public DbSet<User> Users { get; set; }
    public DbSet<Establishment> Establishments { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Cart> Carts { get; set; }
}