using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Kitchen;
using OrderManagement.Domain.Identity;
using Serilog;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{SystemRoles.Kitchen},{SystemRoles.Manager}")]
public class KitchenController(
    IKitchenService kitchenService,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue(CancellationToken cancellationToken)
    {
        try
        {
            var orders = await kitchenService.GetQueueAsync(cancellationToken);
            return Ok(orders);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving kitchen queue");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the kitchen queue.");
        }
    }
}

