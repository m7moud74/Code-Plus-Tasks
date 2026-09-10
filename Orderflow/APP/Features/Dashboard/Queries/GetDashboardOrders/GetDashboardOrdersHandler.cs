using APP.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace APP.Features.Dashboard.Queries.GetDashboardOrders;

public class GetDashboardOrdersHandler(IAppDbContext context) : IRequestHandler<GetDashboardOrdersQuery, List<DashboardOrderDto>>
{
    public async Task<List<DashboardOrderDto>> Handle(GetDashboardOrdersQuery request, CancellationToken cancellationToken)
    {
        return await context.OrderDashboards
            .AsNoTracking()
            .OrderByDescending(d => d.OrderId)
            .Select(d => new DashboardOrderDto(
                d.OrderId,
                d.CustomerName,
                d.ItemCount,
                d.TotalAmount,
                d.Status,
                d.LastRefreshedAt
            ))
            .ToListAsync(cancellationToken);
    }
}
