using System.Diagnostics;
using APP.Common.Diagnostics;
using APP.Common.Interfaces;
using APP.Common.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace APP.Features.Orders.Query;

public class GetOrderHandler(
    IAppDbContext context,
    ICacheService cacheService,
    ILogger<GetOrderHandler> logger
) : IRequestHandler<GetOrderQuery, Result<GetOrderResult>>
{
    public async Task<Result<GetOrderResult>> Handle(
        GetOrderQuery request,
        CancellationToken cancellationToken)
    {
        using var activity =
            DiagnosticsConfig.ActivitySource.StartActivity("GetOrderOperation");

        activity?.SetTag("order.id", request.OrderId);

        logger.LogInformation(
            "Getting order {OrderId}",
            request.OrderId);

        try
        {
            var cacheKey = $"orders:{request.OrderId}";

            // 1. Check Redis Cache first (Cache-Aside Pattern)
            var cachedOrder =
                await cacheService.GetAsync<GetOrderResult>(
                    cacheKey,
                    cancellationToken);

            if (cachedOrder != null)
            {
                activity?.SetTag("cache.hit", true);

                logger.LogInformation(
                    "Cache hit for order {OrderId}",
                    request.OrderId);

                return Result<GetOrderResult>.Success(cachedOrder);
            }

            // Cache miss
            activity?.SetTag("cache.hit", false);

            logger.LogInformation(
                "Cache miss for order {OrderId}",
                request.OrderId);

            // 2. Query Database if cache miss
            var order = await context.Orders
                .AsNoTracking()
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(
                    o => o.Id == request.OrderId,
                    cancellationToken);

            if (order == null)
            {
                logger.LogWarning(
                    "Order {OrderId} was not found",
                    request.OrderId);

                return Result<GetOrderResult>.Failure(
                    $"Order with ID {request.OrderId} not found.");
            }

            var orderItems = order.OrderItems
                .Select(oi => new GetOrderItemDto(
                    oi.ProductId,
                    oi.Product?.Name ?? string.Empty,
                    oi.Quantity,
                    oi.Price
                ))
                .ToList();

            var result = new GetOrderResult(
                order.Id,
                order.CustomerName,
                order.TotalAmount,
                order.Status.ToString(),
                orderItems
            );

            // 3. Store in Redis Cache with 5 minutes expiration
            await cacheService.SetAsync(
                cacheKey,
                result,
                TimeSpan.FromMinutes(5),
                cancellationToken);

            logger.LogInformation(
                "Order {OrderId} loaded from database and cached",
                request.OrderId);

            return Result<GetOrderResult>.Success(result);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(
                ActivityStatusCode.Error,
                ex.Message);

            logger.LogError(
                ex,
                "Error while getting order {OrderId}",
                request.OrderId);

            throw;
        }
    }
}