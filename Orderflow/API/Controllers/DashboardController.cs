using MediatR;
using Microsoft.AspNetCore.Mvc;
using APP.Features.Dashboard.Queries.GetDashboardOrders;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController(ISender mediator) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<IActionResult> GetDashboardOrders(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDashboardOrdersQuery(), cancellationToken);
        return Ok(result);
    }
}
