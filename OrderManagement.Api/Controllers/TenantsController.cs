using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Tenants.Commands;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TenantsController(IMediator mediator, IPlanCatalog planCatalog) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command, CancellationToken cancellationToken)
    {
        var tenant = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetTenantPlans), new { tenant.Id }, tenant);
    }

    [HttpGet("plans")]
    [AllowAnonymous]
    public IActionResult GetTenantPlans()
    {
        var plans = planCatalog.GetPlans();
        return Ok(plans);
    }
}


