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

public class KitchenController(
    IKitchenService kitchenService,
    IKitchenPrintService kitchenPrintService,
    Serilog.ILogger logger) : ControllerBase
{
    [Authorize(Roles = $"{SystemRoles.Waiter},{SystemRoles.Kitchen},{SystemRoles.Manager}")]
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

    /// <summary>
    /// Get kitchen ticket data for an order (for frontend printing)
    /// </summary>
    [HttpGet("orders/{orderId:guid}/ticket")]
    [Authorize(Roles = $"{SystemRoles.Waiter},{SystemRoles.Kitchen},{SystemRoles.Manager}")]
    public async Task<IActionResult> GetKitchenTicket(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var ticket = await kitchenPrintService.GenerateKitchenTicketAsync(orderId, cancellationToken);
            if (ticket is null)
            {
                return NotFound(new { Message = "Order not found" });
            }
            return Ok(ticket);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error generating kitchen ticket for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while generating the kitchen ticket." });
        }
    }

    /// <summary>
    /// Print kitchen ticket for an order
    /// </summary>
    [HttpPost("orders/{orderId:guid}/print")]
    [Authorize(Roles = $"{SystemRoles.Waiter},{SystemRoles.Kitchen},{SystemRoles.Manager}")]
    public async Task<IActionResult> PrintKitchenTicket(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var result = await kitchenPrintService.PrintKitchenTicketAsync(orderId, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }
            return Ok(new 
            { 
                printJobId = result.PrintJobId,
                message = result.Message,
                ticket = result.Ticket
            });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error printing kitchen ticket for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while printing the kitchen ticket." });
        }
    }

    /// <summary>
    /// Reprint kitchen ticket for an order (with audit reason)
    /// </summary>
    [HttpPost("orders/{orderId:guid}/reprint")]
    [Authorize(Roles = $"{SystemRoles.Waiter},{SystemRoles.Kitchen},{SystemRoles.Manager}")]
    public async Task<IActionResult> ReprintKitchenTicket(Guid orderId, [FromBody] ReprintKitchenTicketRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                return BadRequest(new { Message = "Reason is required for reprinting kitchen ticket." });
            }

            var result = await kitchenPrintService.ReprintKitchenTicketAsync(orderId, request.Reason, cancellationToken);
            if (!result.Success)
            {
                return BadRequest(new { Message = result.Message });
            }
            return Ok(new 
            { 
                printJobId = result.PrintJobId,
                message = result.Message,
                ticket = result.Ticket
            });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error reprinting kitchen ticket for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while reprinting the kitchen ticket." });
        }
    }
}

public record ReprintKitchenTicketRequest(string Reason);

