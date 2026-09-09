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
    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder(int id, CancellationToken cancellationToken)
    {  var result = await mediator.Send(new GetOrderQuery(id), cancellationToken);
        if (result == null)
        {
            return NotFound(new { message = $"Order with ID {id} not found." });
        }
        return Ok(result);
    }
}
