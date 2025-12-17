using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Contracts.Payments;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Payments;
using OrderManagement.Domain.Identity;
using Serilog;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class PaymentsController(
    IPaymentService paymentService,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpPost("orders/{orderId:guid}/pay")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> ProcessPayment(
        Guid orderId,
        [FromBody] ProcessPaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            var payment = await paymentService.ProcessPaymentAsync(
                orderId,
                request.Amount,
                request.Provider,
                request.Metadata ?? new Dictionary<string, string>(),
                cancellationToken);

            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Payment processing failed for order {OrderId}: {Message}", orderId, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error processing payment for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the payment.");
        }
    }

    [HttpPost("{paymentId:guid}/refund")]
    [Authorize(Roles = $"{SystemRoles.Manager}")]
    public async Task<IActionResult> RefundPayment(
        Guid paymentId,
        [FromBody] RefundPaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            var payment = await paymentService.RefundPaymentAsync(
                paymentId,
                request.Amount,
                request.Reason,
                cancellationToken);

            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Refund failed for payment {PaymentId}: {Message}", paymentId, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error refunding payment {PaymentId}", paymentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing the refund.");
        }
    }

    [HttpPost("{paymentId:guid}/void")]
    [Authorize(Roles = $"{SystemRoles.Manager}")]
    public async Task<IActionResult> VoidPayment(
        Guid paymentId,
        [FromBody] VoidPaymentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            var payment = await paymentService.VoidPaymentAsync(
                paymentId,
                request.Reason,
                cancellationToken);

            return Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Void failed for payment {PaymentId}: {Message}", paymentId, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error voiding payment {PaymentId}", paymentId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while voiding the payment.");
        }
    }

    [HttpGet("orders/{orderId:guid}")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> GetPaymentByOrderId(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var payment = await paymentService.GetPaymentByOrderIdAsync(
                orderId,
                cancellationToken);

            return payment is null ? NotFound() : Ok(payment);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error retrieving payment for order {OrderId}: {Message}", orderId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving payment for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the payment.");
        }
    }
}

