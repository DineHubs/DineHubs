using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Reporting;
using OrderManagement.Application.Abstractions;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class ReportsController(IReportingService reportingService, ITenantContext tenantContext) : ControllerBase
{
    [HttpGet("sales")]
    public async Task<IActionResult> GetSales([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken cancellationToken)
    {
        var result = await reportingService.GetSalesSummaryAsync(tenantContext.TenantId, tenantContext.BranchId, from, to, cancellationToken);
        return Ok(result);
    }

    [HttpGet("inventory")]
    public async Task<IActionResult> GetInventory(CancellationToken cancellationToken)
    {
        var result = await reportingService.GetInventoryForecastAsync(tenantContext.TenantId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("subscription")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetSubscriptionUsage(CancellationToken cancellationToken)
    {
        var result = await reportingService.GetSubscriptionUsageAsync(tenantContext.TenantId, cancellationToken);
        return Ok(result);
    }
}


