using Microsoft.EntityFrameworkCore;

namespace APP.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Product> Products { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<OrderDashboardReadModel> OrderDashboards { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
