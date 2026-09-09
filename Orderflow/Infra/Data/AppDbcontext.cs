using Microsoft.EntityFrameworkCore;

public class AppDbcontext(DbContextOptions<AppDbcontext> options) : DbContext(options)
{
    public DbSet<Product> Products { get; set; } = default!;
    public DbSet<Order> Orders { get; set; } = default!;
    public DbSet<OrderItem> OrderItems { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(p => p.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.Property(oi => oi.Price).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2);
        });

        modelBuilder.SeedData();
    }
}