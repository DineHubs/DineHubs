using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Contracts.Orders;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Ordering;
using OrderManagement.Application.Ordering.Models;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Identity;
using Serilog;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class OrdersController(
    IOrderService orderService,
    ITenantContext tenantContext,
    IQrOrderingService qrOrderingService,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Check model binding/validation errors
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => new { Field = x.Key, Message = e.ErrorMessage }))
                    .ToList();
                
                logger.Warning("Order creation failed due to model validation errors: {Errors}", 
                    string.Join(", ", errors.Select(e => $"{e.Field}: {e.Message}")));
                
                return BadRequest(new { 
                    Message = "Invalid request data.",
                    Errors = errors.Select(e => new { e.Field, e.Message })
                });
            }

            if (request is null)
            {
                logger.Warning("Order creation failed: Request body is null");
                return BadRequest(new { Message = "Request body is required." });
            }

            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            var dto = new CreateOrderDto(
                request.IsTakeAway,
                request.TableNumber,
                request.Items.Select(i => new OrderLineDto(i.MenuItemId, i.Name, i.Price, i.Quantity)).ToList());

            var order = await orderService.CreateOrderAsync(
                dto, 
                tenantContext.TenantId, 
                tenantContext.BranchId.Value, 
                cancellationToken);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Order creation failed: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (FormatException ex)
        {
            logger.Warning("Order creation failed due to format error (likely invalid GUID): {Message}", ex.Message);
            return BadRequest(new { Message = "Invalid data format. Please check that all menu item IDs are valid GUIDs." });
        }
        catch (System.Text.Json.JsonException ex)
        {
            logger.Warning("Order creation failed due to JSON deserialization error: {Message}", ex.Message);
            return BadRequest(new { Message = "Invalid JSON format. Please check the request data." });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error creating order. Request: IsTakeAway={IsTakeAway}, ItemsCount={ItemsCount}", 
                request?.IsTakeAway, request?.Items?.Count ?? 0);
            return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "An error occurred while creating the order." });
        }
    }

    [HttpGet]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        try
        {
            var orders = await orderService.GetOrdersAsync(
                tenantContext.TenantId, 
                tenantContext.BranchId, 
                cancellationToken);
            
            return Ok(orders);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error retrieving orders: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving orders");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving orders.");
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(
                id, 
                tenantContext.TenantId, 
                tenantContext.BranchId, 
                cancellationToken);
            
            return order is null ? NotFound() : Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error retrieving order {OrderId}: {Message}", id, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order.");
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Kitchen}")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] OrderStatus status, CancellationToken cancellationToken)
    {
        try
        {
            await orderService.UpdateOrderStatusAsync(
                id, 
                status, 
                tenantContext.TenantId, 
                tenantContext.BranchId, 
                cancellationToken);
            
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { Message = ex.Message });
            }
            logger.Warning("Error updating order status: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error updating order {OrderId} status", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order status.");
        }
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            await orderService.CancelOrderAsync(
                id,
                request.Reason,
                tenantContext.TenantId,
                tenantContext.BranchId.Value,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { Message = ex.Message });
            }
            logger.Warning("Order cancellation failed for {OrderId}: {Message}", id, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error cancelling order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while cancelling the order.");
        }
    }

    [HttpDelete("{orderId:guid}/lines/{lineId:guid}")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> RemoveOrderLine(Guid orderId, Guid lineId, CancellationToken cancellationToken)
    {
        try
        {
            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            await orderService.RemoveOrderLineAsync(
                orderId,
                lineId,
                tenantContext.TenantId,
                tenantContext.BranchId.Value,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { Message = ex.Message });
            }
            logger.Warning("Error removing line {LineId} from order {OrderId}: {Message}", lineId, orderId, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error removing line {LineId} from order {OrderId}", lineId, orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while removing the order line.");
        }
    }

    [HttpPatch("{orderId:guid}/lines/{lineId:guid}")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> UpdateOrderLineQuantity(Guid orderId, Guid lineId, [FromBody] UpdateOrderLineRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (tenantContext.BranchId is null)
            {
                return BadRequest(new { Message = "Branch context is required." });
            }

            await orderService.UpdateOrderLineQuantityAsync(
                orderId,
                lineId,
                request.Quantity,
                tenantContext.TenantId,
                tenantContext.BranchId.Value,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                return NotFound(new { Message = ex.Message });
            }
            logger.Warning("Error updating line {LineId} quantity in order {OrderId}: {Message}", lineId, orderId, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error updating line {LineId} quantity in order {OrderId}", lineId, orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the order line quantity.");
        }
    }

    [HttpPost("qr")]
    [AllowAnonymous]
    public async Task<IActionResult> GenerateQrSession([FromBody] string tableNumber, CancellationToken cancellationToken)
    {
        if (tenantContext.BranchId is null)
        {
            return BadRequest("Branch context is required.");
        }

        var code = await qrOrderingService.GenerateSessionAsync(tenantContext.TenantId, tenantContext.BranchId.Value, tableNumber, cancellationToken);
        return Ok(new { Code = code });
    }

    [HttpGet("{id:guid}/can-submit-to-kitchen")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter},{SystemRoles.Kitchen}")]
    public async Task<IActionResult> CanSubmitToKitchen(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await orderService.CanSubmitToKitchenAsync(
                id,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            return Ok(new
            {
                canSubmit = result.CanSubmit,
                requiresPayment = result.RequiresPayment,
                paymentStatus = result.PaymentStatus,
                message = result.Message
            });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error checking kitchen submission eligibility for order {OrderId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while checking kitchen submission eligibility." });
        }
    }
}


