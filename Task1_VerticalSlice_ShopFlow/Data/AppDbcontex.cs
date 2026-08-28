using Microsoft.EntityFrameworkCore;

public class AppDbcontext(DbContextOptions<AppDbcontext> options ):DbContext(options)
{
    public DbSet<Order> orders { get; set; }
    public DbSet<Product> products { get; set; }
    public DbSet<Cart> carts{ get; set; }
}