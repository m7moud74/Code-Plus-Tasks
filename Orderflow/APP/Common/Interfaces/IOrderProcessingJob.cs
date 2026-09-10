namespace APP.Common.Interfaces;

public interface IOrderProcessingJob
{
    Task ProcessPendingOrdersAsync(CancellationToken cancellationToken = default);
    Task RefreshOrderDashboardAsync(CancellationToken cancellationToken = default);
}