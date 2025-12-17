using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Contracts.Receipts;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Receipts;
using OrderManagement.Domain.Identity;
using Serilog;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReceiptsController(
    IReceiptService receiptService,
    ITenantContext tenantContext,
    ICurrentUserContext currentUserContext,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet("orders/{orderId:guid}")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> GetReceipt(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var receiptUrl = await receiptService.GetReceiptUrlAsync(orderId, cancellationToken);
            
            if (string.IsNullOrEmpty(receiptUrl))
            {
                return NotFound(new { Message = "Receipt not found for this order." });
            }

            return Ok(new { ReceiptUrl = receiptUrl });
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error retrieving receipt for order {OrderId}: {Message}", orderId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving receipt for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the receipt.");
        }
    }

    [HttpPost("orders/{orderId:guid}/generate")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> GenerateReceipt(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            var receiptUrl = await receiptService.GenerateReceiptAsync(orderId, cancellationToken);
            
            return Ok(new { ReceiptUrl = receiptUrl });
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error generating receipt for order {OrderId}: {Message}", orderId, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error generating receipt for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while generating the receipt.");
        }
    }

    [HttpPost("orders/{orderId:guid}/reprint")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> ReprintReceipt(
        Guid orderId,
        [FromBody] ReprintReceiptRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            var receiptUrl = await receiptService.ReprintReceiptAsync(
                orderId,
                request.Reason,
                currentUserContext.UserId,
                cancellationToken);
            
            return Ok(new { ReceiptUrl = receiptUrl });
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error reprinting receipt for order {OrderId}: {Message}", orderId, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error reprinting receipt for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while reprinting the receipt.");
        }
    }
}

