using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Tenants.Commands;
using OrderManagement.Application.Users;
using OrderManagement.Domain.Identity;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TenantsController(IMediator mediator, IPlanCatalog planCatalog, IUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<IActionResult> GetTenants([FromServices] OrderManagement.Infrastructure.Persistence.OrderManagementDbContext dbContext, CancellationToken cancellationToken)
    {
        var tenants = await dbContext.Tenants
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync(cancellationToken);
        return Ok(tenants);
    }

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

    [HttpGet("{tenantId:guid}/users")]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<IActionResult> GetTenantUsers(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var users = await userService.GetUsersAsync(tenantId, cancellationToken);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users.");
        }
    }
}


