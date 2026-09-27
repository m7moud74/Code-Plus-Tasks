using System.Diagnostics;
using APP.Common.Diagnostics;
using APP.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infra.BackgroudJobs;

public class OrderProcessingJob(
    IAppDbContext context,
    ICacheService cacheService,
    OrderMetrics orderMetrics,
    ILogger<OrderProcessingJob> logger
) : IOrderProcessingJob
{
    public async Task ProcessPendingOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        using var activity =
            DiagnosticsConfig.ActivitySource
                .StartActivity("ProcessPendingOrders");

        logger.LogInformation(
            "Starting pending orders processing");

        try
        {
            var pendingOrders = await context.Orders
                .Where(o => o.Status == OrderStatus.Pending)
                .ToListAsync(cancellationToken);

            activity?.SetTag(
                "orders.pending_count",
                pendingOrders.Count);

            if (pendingOrders.Count == 0)
            {
                logger.LogInformation(
                    "No pending orders found");

                return;
            }

            foreach (var order in pendingOrders)
            {
                order.Status = OrderStatus.Completed;

                // Invalidate cache for the updated order
                await cacheService.RemoveAsync(
                    $"orders:{order.Id}",
                    cancellationToken);

                orderMetrics.OrderProcessed();
            }

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Processed {Count} pending order(s) to Completed and invalidated their cache.",
                pendingOrders.Count);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error while processing pending orders");

            throw;
        }
    }

    public async Task RefreshOrderDashboardAsync(
        CancellationToken cancellationToken = default)
    {
        using var activity =
            DiagnosticsConfig.ActivitySource
                .StartActivity("RefreshOrderDashboard");

        logger.LogInformation(
            "Starting order dashboard refresh");

        try
        {
            // 1. Process pending orders first
            await ProcessPendingOrdersAsync(cancellationToken);

            // 2. Fetch aggregated order data
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

            activity?.SetTag(
                "dashboard.records_count",
                newDashboards.Count);

            // 3. Replace existing materialized view records
            var existingDashboards =
                await context.OrderDashboards
                    .ToListAsync(cancellationToken);

            if (existingDashboards.Count > 0)
            {
                context.OrderDashboards.RemoveRange(
                    existingDashboards);
            }

            await context.OrderDashboards.AddRangeAsync(
                newDashboards,
                cancellationToken);

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Materialized View (OrderDashboards) refreshed with {Count} records.",
                newDashboards.Count);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error while refreshing order dashboard");

            throw;
        }
    }
}