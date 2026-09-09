using APP.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetOrderHandler(IAppDbContext context) : IRequestHandler<GetOrderQuery, GetOrderResult?>
{
    public async Task<GetOrderResult?> Handle(GetOrderQuery request, CancellationToken cancellationToken)
    {
        var order = await context.Orders.AsNoTracking()
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

        if (order == null)
        {
            return null;
        }

        var orderItems = order.OrderItems.Select(oi => new GetOrderItemDto(
            oi.ProductId,
            oi.Product.Name,
            oi.Quantity,
            oi.Price
        )).ToList();

        return new GetOrderResult(
            order.CustomerName,
            order.TotalAmount,
            order.Status.ToString(),
            orderItems
        );
    }
}