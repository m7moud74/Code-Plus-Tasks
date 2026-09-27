using System.Diagnostics.Metrics;

namespace APP.Common.Diagnostics;

public class OrderMetrics
{
    private readonly Counter<long> _ordersCreatedCounter;
    private readonly Counter<long> _workerProcessedCounter;

    public OrderMetrics()
    {
        _ordersCreatedCounter =
            DiagnosticsConfig.Meter.CreateCounter<long>(
                name: "orderflow_orders_created_total",
                description: "Total number of orders created");

        _workerProcessedCounter =
            DiagnosticsConfig.Meter.CreateCounter<long>(
                name: "orderflow_worker_processed_orders_total",
                description: "Total orders processed by the background worker");
    }

    public void OrderCreated()
        => _ordersCreatedCounter.Add(1);

    public void OrderProcessed()
        => _workerProcessedCounter.Add(1);
}