using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{SystemRoles.SuperAdmin},{SystemRoles.Admin}")]
public class SubscriptionsController(ISubscriptionService subscriptionService) : ControllerBase
{
    [HttpPost("{tenantId:guid}/upgrade")]
    public async Task<IActionResult> RequestPlanChange(Guid tenantId, [FromQuery] SubscriptionPlanCode plan, CancellationToken cancellationToken)
    {
        await subscriptionService.RequestPlanChangeAsync(tenantId, plan, cancellationToken);
        return Accepted();
    }
}


