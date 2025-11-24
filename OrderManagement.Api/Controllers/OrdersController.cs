using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Api.Contracts.Orders;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Ordering;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Identity;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Waiter}")]
public class OrdersController(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    IQrOrderingService qrOrderingService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken cancellationToken)
    {
        if (tenantContext.BranchId is null)
        {
            return BadRequest("Branch context is required.");
        }

        // Validate TableNumber: required for dine-in orders, optional for takeaway
        if (!request.IsTakeAway && string.IsNullOrWhiteSpace(request.TableNumber))
        {
            return BadRequest("Table number is required for dine-in orders.");
        }

        // Use empty string for takeaway orders when TableNumber is null
        var tableNumber = request.IsTakeAway 
            ? (request.TableNumber ?? string.Empty)
            : request.TableNumber!;

        var order = new Order(
            tenantContext.TenantId,
            tenantContext.BranchId.Value,
            $"OM-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}",
            request.IsTakeAway,
            tableNumber);

        foreach (var item in request.Items)
        {
            order.AddLine(item.MenuItemId, item.Name, item.Price, item.Quantity);
        }

        // Set order status to Submitted so it appears in kitchen queue
        order.UpdateStatus(OrderStatus.Submitted);

        dbContext.Orders.Add(order);
        await dbContext.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders(CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.AsNoTracking()
            .Where(o => o.TenantId == tenantContext.TenantId);

        if (tenantContext.BranchId.HasValue)
        {
            query = query.Where(o => o.BranchId == tenantContext.BranchId.Value);
        }

        var orders = await query
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
        
        return Ok(orders);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.AsNoTracking().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{SystemRoles.SuperAdmin},{SystemRoles.Kitchen},{SystemRoles.Manager}")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromQuery] OrderStatus status, CancellationToken cancellationToken)
    {
        var order = await dbContext.Orders.FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (order is null)
        {
            return NotFound();
        }

        order.UpdateStatus(status);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("qr")]
    public async Task<IActionResult> GenerateQrSession([FromBody] string tableNumber, CancellationToken cancellationToken)
    {
        if (tenantContext.BranchId is null)
        {
            return BadRequest("Branch context is required.");
        }

        var code = await qrOrderingService.GenerateSessionAsync(tenantContext.TenantId, tenantContext.BranchId.Value, tableNumber, cancellationToken);
        return Ok(new { Code = code });
    }
}


