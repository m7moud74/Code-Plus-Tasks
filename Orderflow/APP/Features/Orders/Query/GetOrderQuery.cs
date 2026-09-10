using MediatR;
using APP.Common.Models;

public record GetOrderQuery(int OrderId) : IRequest<GetOrderResult?>;
public record GetAllOrdersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<GetOrderResult>>;

public record GetOrderResult(int Id, string CustomerName, decimal TotalAmount, string Status, List<GetOrderItemDto> Items);

public record GetOrderItemDto(int ProductId, string ProductName, int Quantity, decimal Price);