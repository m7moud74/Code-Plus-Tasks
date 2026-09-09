using MediatR;
using Microsoft.EntityFrameworkCore;
using APP.Common.Interfaces;

namespace APP.Features.Orders.Commands.CreateOrder;

public class CreateOrderHandler(IAppDbContext context) : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.CustomerName))
        {
            throw new ArgumentException("Customer name is required.");
        }

        if (request.Items == null || request.Items.Count == 0)
        {
            throw new ArgumentException("An order must contain at least one item.");
        }

        var productIds = request.Items.Select(i => i.ProductId).Distinct().ToList();
        var products = await context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken);

        if (products.Count != productIds.Count)
        {
            throw new InvalidOperationException("One or more products were not found.");
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
            if (itemDto.Quantity <= 0)
            {
                throw new ArgumentException($"Quantity for product {itemDto.ProductId} must be greater than zero.");
            }

            var product = products.First(p => p.Id == itemDto.ProductId);

            if (product.Quantity < itemDto.Quantity)
            {
                throw new InvalidOperationException($"Insufficient stock for product '{product.Name}'. Available: {product.Quantity}, Requested: {itemDto.Quantity}.");
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

        return new CreateOrderResult(order.Id, order.TotalAmount, order.Status.ToString());
    }
}
