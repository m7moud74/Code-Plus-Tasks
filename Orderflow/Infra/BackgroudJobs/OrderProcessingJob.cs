using APP.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infra.BackgroudJobs;

public class OrderProcessingJob(
    IAppDbContext context,
    ILogger<OrderProcessingJob> logger
) : IOrderProcessingJob
{
    public async Task ProcessPendingOrdersAsync(CancellationToken cancellationToken = default)
    {
        var pendingOrders = await context.Orders
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(cancellationToken);

        if (pendingOrders.Count == 0)
        {
            return;
        }

        foreach (var order in pendingOrders)
        {
            order.Status = OrderStatus.Completed;
        }

        await context.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Processed {Count} pending order(s) to Completed.", pendingOrders.Count);
    }

    public async Task RefreshOrderDashboardAsync(CancellationToken cancellationToken = default)
    {
        // 1. Ensure any pending orders are processed first
        await ProcessPendingOrdersAsync(cancellationToken);

        // 2. Fetch aggregated order data for the Materialized View
        var refreshedAt = DateTime.UtcNow;

        var newDashboards = await context.Orders
            .AsNoTracking()
            .Select(order => new OrderDashboardReadModel
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                ItemCount = order.OrderItems.Sum(oi => oi.Quantity),
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                LastRefreshedAt = refreshedAt
            })
            .ToListAsync(cancellationToken);

        // 3. Replace existing records in the Materialized View table
        var existingDashboards = await context.OrderDashboards.ToListAsync(cancellationToken);
        if (existingDashboards.Count > 0)
        {
            context.OrderDashboards.RemoveRange(existingDashboards);
        }

        await context.OrderDashboards.AddRangeAsync(newDashboards, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Materialized View (OrderDashboards) refreshed with {Count} records.", newDashboards.Count);
    }
}