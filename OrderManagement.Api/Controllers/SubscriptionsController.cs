using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Identity;
using Serilog;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = SystemRoles.SuperAdmin)]
public class SubscriptionsController(
    ISubscriptionService subscriptionService,
    IPlanCatalog planCatalog,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllSubscriptions(CancellationToken cancellationToken)
    {
        try
        {
            var subscriptions = await subscriptionService.GetAllSubscriptionsAsync(cancellationToken);
            return Ok(subscriptions);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving all subscriptions");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving subscriptions.");
        }
    }

    [HttpGet("tenant/{tenantId:guid}")]
    public async Task<IActionResult> GetSubscriptionByTenant(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await subscriptionService.GetSubscriptionByTenantIdAsync(tenantId, cancellationToken);
            if (subscription == null)
            {
                return NotFound();
            }
            return Ok(subscription);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving subscription for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the subscription.");
        }
    }

    [HttpGet("plans")]
    public IActionResult GetPlans()
    {
        try
        {
            var plans = planCatalog.GetPlans();
            return Ok(plans);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving subscription plans");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving subscription plans.");
        }
    }

    [HttpPost("{tenantId:guid}/upgrade")]
    public async Task<IActionResult> RequestPlanChange(Guid tenantId, [FromQuery] SubscriptionPlanCode plan, CancellationToken cancellationToken)
    {
        try
        {
            await subscriptionService.RequestPlanChangeAsync(tenantId, plan, cancellationToken);
            return Accepted();
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error requesting plan change for tenant {TenantId}: {Message}", tenantId, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error requesting plan change for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while requesting plan change.");
        }
    }
}


