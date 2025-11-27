using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Tenants;
using OrderManagement.Application.Tenants.Commands;
using OrderManagement.Application.Users;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TenantsController(
    IMediator mediator, 
    IPlanCatalog planCatalog, 
    IUserService userService,
    ITenantService tenantService,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<IActionResult> GetTenants(CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await tenantService.GetAllTenantsAsync(cancellationToken);
            return Ok(tenants);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error retrieving tenants: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving tenants");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving tenants.");
        }
    }

    [HttpPost]
    [Authorize(Roles = SystemRoles.SuperAdmin)]
    public async Task<IActionResult> CreateTenant([FromBody] CreateTenantCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await mediator.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetTenantPlans), new { tenant.Id }, tenant);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error creating tenant: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error creating tenant");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the tenant.");
        }
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
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error retrieving users for tenant {TenantId}: {Message}", tenantId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving users for tenant {TenantId}", tenantId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users.");
        }
    }
}


