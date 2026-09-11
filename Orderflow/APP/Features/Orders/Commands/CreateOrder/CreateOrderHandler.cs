using MediatR;
using Microsoft.EntityFrameworkCore;
using APP.Common.Interfaces;
using APP.Common.Results;

namespace APP.Features.Orders.Commands.CreateOrder;

public class CreateOrderHandler(
    IAppDbContext context,
    IBackgroundJobService backgroundJobService
) : IRequestHandler<CreateOrderCommand, Result<CreateOrderResult>>
{
    public async Task<Result<CreateOrderResult>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            return Result<CreateOrderResult>.Failure("One or more products were not found.");
        }

        var order = new Order
        {
            CustomerName = request.CustomerName.Trim(),
            Status = OrderStatus.Pending,
            TotalAmount = 0
        };

        decimal calculatedTotal = 0;

        foreach (var itemDto in request.Items)
        {
            var product = products.First(p => p.Id == itemDto.ProductId);

            if (product.Quantity < itemDto.Quantity)
            {
                return Result<CreateOrderResult>.Failure(
                    $"Insufficient stock for product '{product.Name}'. Available: {product.Quantity}, Requested: {itemDto.Quantity}."
                );
            }

            // Decrement inventory stock
            product.Quantity -= itemDto.Quantity;

            var orderItem = new OrderItem
            {
                ProductId = product.Id,
                Quantity = itemDto.Quantity,
                Price = product.Price
            };

            order.OrderItems.Add(orderItem);
            calculatedTotal += product.Price * itemDto.Quantity;
        }

        order.TotalAmount = calculatedTotal;

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        // Schedule background job to process the pending order and refresh dashboard after 10 seconds
        backgroundJobService.Schedule<IOrderProcessingJob>(
            job => job.RefreshOrderDashboardAsync(default),
            TimeSpan.FromSeconds(10)
        );

        return Result<CreateOrderResult>.Success(
            new CreateOrderResult(order.Id, order.TotalAmount, order.Status.ToString())
        );
    }
}
