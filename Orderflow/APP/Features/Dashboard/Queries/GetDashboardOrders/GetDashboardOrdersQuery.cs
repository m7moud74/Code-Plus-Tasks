using MediatR;

namespace APP.Features.Dashboard.Queries.GetDashboardOrders;

public record GetDashboardOrdersQuery : IRequest<List<DashboardOrderDto>>;

public record DashboardOrderDto(
    int OrderId,
    string CustomerName,
    int ItemCount,
    decimal TotalAmount,
    string Status,
    DateTime LastRefreshedAt
);
