using Microsoft.EntityFrameworkCore;

public static class ModelBuilderExtensions
{
    public static void SeedData(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>().HasData(
            new Product { Id = 1, Name = "Laptop", Price = 1200.00m, Quantity = 50 },
            new Product { Id = 2, Name = "Smartphone", Price = 800.00m, Quantity = 100 },
            new Product { Id = 3, Name = "Wireless Headphones", Price = 150.00m, Quantity = 75 },
            new Product { Id = 4, Name = "Mechanical Keyboard", Price = 90.00m, Quantity = 60 },
            new Product { Id = 5, Name = "Gaming Mouse", Price = 45.00m, Quantity = 120 }
        );

        modelBuilder.Entity<Order>().HasData(
            new Order { Id = 1, CustomerName = "Ahmed Ali", Status = OrderStatus.Completed, TotalAmount = 1350.00m },
            new Order { Id = 2, CustomerName = "Sara Mohamed", Status = OrderStatus.Pending, TotalAmount = 890.00m },
            new Order { Id = 3, CustomerName = "Omar Khaled", Status = OrderStatus.Pending, TotalAmount = 195.00m }
        );

        modelBuilder.Entity<OrderItem>().HasData(
            new OrderItem { Id = 1, OrderId = 1, ProductId = 1, Quantity = 1, Price = 1200.00m },
            new OrderItem { Id = 2, OrderId = 1, ProductId = 3, Quantity = 1, Price = 150.00m },
            new OrderItem { Id = 3, OrderId = 2, ProductId = 2, Quantity = 1, Price = 800.00m },
            new OrderItem { Id = 4, OrderId = 2, ProductId = 4, Quantity = 1, Price = 90.00m },
            new OrderItem { Id = 5, OrderId = 3, ProductId = 3, Quantity = 1, Price = 150.00m },
            new OrderItem { Id = 6, OrderId = 3, ProductId = 5, Quantity = 1, Price = 45.00m }
        );
    }
}
