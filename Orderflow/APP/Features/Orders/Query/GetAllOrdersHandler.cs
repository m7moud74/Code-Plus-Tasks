using APP.Common.Interfaces;
using APP.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetAllOrdersHandler(IAppDbContext context) : IRequestHandler<GetAllOrdersQuery, PagedResult<GetOrderResult>>
{
    public async Task<PagedResult<GetOrderResult>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 10 : (request.PageSize > 100 ? 100 : request.PageSize);

        var totalCount = await context.Orders.CountAsync(cancellationToken);

        var orders = await context.Orders.AsNoTracking()
            .OrderByDescending(o => o.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ToListAsync(cancellationToken);

        var orderResults = orders.Select(order =>
        {
            var orderItems = order.OrderItems.Select(oi => new GetOrderItemDto(
                oi.ProductId,
                oi.Product?.Name ?? string.Empty,
                oi.Quantity,
                oi.Price
            )).ToList();

            return new GetOrderResult(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.Status.ToString(),
                orderItems
            );
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResult<GetOrderResult>(orderResults, pageNumber, pageSize, totalCount, totalPages);
    }
}
    
