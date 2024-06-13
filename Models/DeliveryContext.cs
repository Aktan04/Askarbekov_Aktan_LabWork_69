using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Delivery.Models;

public class DeliveryContext : IdentityDbContext<User, IdentityRole<int>, int>
{
    public DeliveryContext(DbContextOptions<DeliveryContext> options) : base(options){}

    public DbSet<User> Users { get; set; }
}