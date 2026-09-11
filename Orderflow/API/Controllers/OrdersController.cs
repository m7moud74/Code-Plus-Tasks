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
        if (result.IsFailure)
        {
            return BadRequest(new { error = result.Error });
        }
        return CreatedAtAction(nameof(GetOrder), new { id = result.Value.OrderId }, result.Value);
    }
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetOrder(int id, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetOrderQuery(id), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(new { error = result.Error });
        }
        return Ok(result.Value);
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
