using MediatR;
using APP.Common.Results;

namespace APP.Features.Orders.Commands.CreateOrder;

public record CreateOrderItemDto(int ProductId, int Quantity);

public record CreateOrderCommand(string CustomerName, List<CreateOrderItemDto> Items) : IRequest<Result<CreateOrderResult>>;

public record CreateOrderResult(int OrderId, decimal TotalAmount, string Status);
