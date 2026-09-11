using APP.Common.Interfaces;
using APP.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace APP.Features.Orders.Query;

public class GetOrderHandler(
    IAppDbContext context,
    ICacheService cacheService
) : IRequestHandler<GetOrderQuery, Result<GetOrderResult>>
{
    public async Task<Result<GetOrderResult>> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = $"orders:{request.OrderId}";

        // 1. Check Redis Cache first (Cache-Aside Pattern)
        var cachedOrder = await cacheService.GetAsync<GetOrderResult>(cacheKey, cancellationToken);
        if (cachedOrder != null)
        {
            return Result<GetOrderResult>.Success(cachedOrder);
        }

        // 2. Query Database if cache miss
        var order = await context.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return Result<GetOrderResult>.Failure($"Order with ID {request.OrderId} not found.");
        }

        var orderItems = order.OrderItems.Select(oi => new GetOrderItemDto(
            oi.ProductId,
            oi.Product?.Name ?? string.Empty,
            oi.Quantity,
            oi.Price
        )).ToList();

        var result = new GetOrderResult(
            order.Id,
            order.CustomerName,
            order.TotalAmount,
            order.Status.ToString(),
            orderItems
        );

        // 3. Store in Redis Cache with 5 minutes expiration
        await cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5), cancellationToken);

        return Result<GetOrderResult>.Success(result);
    }
}