using MediatR;
using Microsoft.AspNetCore.Mvc;
using APP.Features.Orders.Commands.CreateOrder;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController(ISender mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        return Ok(result);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrderQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Order with ID {id} not found." });
        }
        return Ok(result);
    }
    [HttpGet]
    public async Task<IActionResult> GetAllOrders(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var result = await mediator.Send(new GetAllOrdersQuery(pageNumber, pageSize), cancellationToken);
        return Ok(result);
    }
}
