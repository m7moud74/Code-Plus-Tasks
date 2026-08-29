using Microsoft.EntityFrameworkCore;

public class AppDbcontext(DbContextOptions<AppDbcontext> options ):DbContext(options)
{
    public DbSet<Order> orders { get; set; }
    public DbSet<Product> products { get; set; }
    public DbSet<Cart> carts { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>().HasData(
            new Product { ProductID = 1, Name = "Laptop" },
            new Product { ProductID = 2, Name = "Mouse" },
            new Product { ProductID = 3, Name = "Keyboard" },
            new Product { ProductID = 4, Name = "Monitor" }
        );
    }
}