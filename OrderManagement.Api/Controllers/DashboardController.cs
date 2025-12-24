using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Dashboard;
using OrderManagement.Domain.Identity;
using Serilog;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class DashboardController(
    IDashboardService dashboardService,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet("stats")]
    [Authorize(Roles = $"{SystemRoles.SuperAdmin},{SystemRoles.Admin},{SystemRoles.Manager},{SystemRoles.Waiter},{SystemRoles.Kitchen},{SystemRoles.InventoryManager}")]
    public async Task<IActionResult> GetStats([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var fromDate = from ?? DateTimeOffset.UtcNow.Date;
            var toDate = to ?? DateTimeOffset.UtcNow.Date.AddDays(1).AddTicks(-1);

            var stats = await dashboardService.GetDashboardStatsAsync(fromDate, toDate, branchId, cancellationToken);
            return Ok(stats);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving dashboard stats for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving dashboard statistics.");
        }
    }

    [HttpGet("sales-trend")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<IActionResult> GetSalesTrend([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var trend = await dashboardService.GetSalesTrendAsync(from, to, branchId, cancellationToken);
            return Ok(trend);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving sales trend for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving sales trend.");
        }
    }

    [HttpGet("top-items")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<IActionResult> GetTopItems(CancellationToken cancellationToken, [FromQuery] int count = 10, [FromQuery] DateTimeOffset? from = null, [FromQuery] DateTimeOffset? to = null, [FromQuery] Guid? branchId = null)
    {
        try
        {
            var fromDate = from ?? DateTimeOffset.UtcNow.AddDays(-30);
            var toDate = to ?? DateTimeOffset.UtcNow;

            var items = await dashboardService.GetTopSellingItemsAsync(count, fromDate, toDate, branchId, cancellationToken);
            return Ok(items);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving top selling items for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving top selling items.");
        }
    }

    [HttpGet("orders-by-status")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager},{SystemRoles.Kitchen}")]
    public async Task<IActionResult> GetOrdersByStatus(CancellationToken cancellationToken, [FromQuery] DateTimeOffset? from = null, [FromQuery] DateTimeOffset? to = null, [FromQuery] Guid? branchId = null)
    {
        try
        {
            var fromDate = from ?? DateTimeOffset.UtcNow.AddDays(-30);
            var toDate = to ?? DateTimeOffset.UtcNow;

            var statusCounts = await dashboardService.GetOrdersByStatusAsync(fromDate, toDate, branchId, cancellationToken);
            return Ok(statusCounts);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving order status counts for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving order status counts.");
        }
    }

    [HttpGet("orders-by-hour")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<IActionResult> GetOrdersByHour(CancellationToken cancellationToken, [FromQuery] DateTimeOffset? date = null, [FromQuery] Guid? branchId = null)
    {
        try
        {
            var targetDate = date ?? DateTimeOffset.UtcNow.Date;

            var hourlyCounts = await dashboardService.GetOrdersByHourAsync(targetDate, branchId, cancellationToken);
            return Ok(hourlyCounts);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving hourly order counts for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving hourly order counts.");
        }
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager},{SystemRoles.InventoryManager}")]
    public async Task<IActionResult> GetLowStockItems(CancellationToken cancellationToken, [FromQuery] Guid? branchId = null)
    {
        try
        {
            var items = await dashboardService.GetLowStockItemsAsync(branchId, cancellationToken);
            return Ok(items);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving low stock items for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving low stock items.");
        }
    }

    [HttpGet("revenue-by-day")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<IActionResult> GetRevenueByDay([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var revenue = await dashboardService.GetRevenueByDayAsync(from, to, branchId, cancellationToken);
            return Ok(revenue);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving revenue by day for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving revenue by day.");
        }
    }

    [HttpGet("average-order-value")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<IActionResult> GetAverageOrderValueTrend([FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, [FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var trend = await dashboardService.GetAverageOrderValueTrendAsync(from, to, branchId, cancellationToken);
            return Ok(trend);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving average order value trend for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving average order value trend.");
        }
    }

    [HttpGet("subscription-status")]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<IActionResult> GetSubscriptionStatusBreakdown(CancellationToken cancellationToken)
    {
        try
        {
            var breakdown = await dashboardService.GetSubscriptionStatusBreakdownAsync(cancellationToken);
            return Ok(breakdown);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving subscription status breakdown");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving subscription status breakdown.");
        }
    }

    [HttpGet("subscription-trend")]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<IActionResult> GetSubscriptionTrend([FromQuery] int months = 6, CancellationToken cancellationToken = default)
    {
        try
        {
            var trend = await dashboardService.GetSubscriptionTrendAsync(months, cancellationToken);
            return Ok(trend);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving subscription trend");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving subscription trend.");
        }
    }
}

