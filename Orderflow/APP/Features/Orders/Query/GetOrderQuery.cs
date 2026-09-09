using MediatR;

public record GetOrderQuery(int OrderId) : IRequest<GetOrderResult?>;

public record GetOrderResult( string CustomerName, decimal TotalAmount, string Status, List<GetOrderItemDto> Items);

public record GetOrderItemDto(int ProductId, string ProductName, int Quantity, decimal Price)
{
}