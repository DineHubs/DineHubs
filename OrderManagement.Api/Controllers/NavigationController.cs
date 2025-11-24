using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Navigation;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class NavigationController(
    INavigationMenuService navigationMenuService,
    ICurrentUserContext currentUserContext,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet("menu")]
    public async Task<ActionResult<IReadOnlyCollection<object>>> GetMenu(CancellationToken cancellationToken)
    {
        try
        {
            var roles = currentUserContext.Roles;
            if (!roles.Any())
            {
                logger.Warning("User {UserId} has no roles assigned", currentUserContext.UserId);
                return Ok(Array.Empty<object>());
            }

            var menu = await navigationMenuService.GetMenuForRolesAsync(roles, cancellationToken);
            logger.Information("Retrieved navigation menu for user {UserId} with {RoleCount} roles, {MenuCount} items", 
                currentUserContext.UserId, roles.Count, menu.Count);
            return Ok(menu);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving navigation menu for user {UserId}", currentUserContext.UserId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the navigation menu.");
        }
    }
}

