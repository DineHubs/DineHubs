using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Identity;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{SystemRoles.SuperAdmin},{SystemRoles.Kitchen},{SystemRoles.Manager}")]
public class KitchenController(OrderManagementDbContext dbContext, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("queue")]
    public async Task<IActionResult> GetQueue(CancellationToken cancellationToken)
    {
        var query = dbContext.Orders.AsNoTracking()
            .Where(o => o.TenantId == tenantContext.TenantId)
            .Where(o =>
                o.Status == OrderStatus.Submitted ||
                o.Status == OrderStatus.InPreparation ||
                o.Status == OrderStatus.Ready);

        if (tenantContext.BranchId.HasValue)
        {
            query = query.Where(o => o.BranchId == tenantContext.BranchId);
        }

        var orders = await query.ToListAsync(cancellationToken);
        return Ok(orders);
    }
}

