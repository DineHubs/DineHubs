using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OrderManagement.Application.MenuManagement;
using OrderManagement.Application.MenuManagement.Models;
using OrderManagement.Api.Contracts.MenuManagement;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/menu-management")]
[Authorize(Roles = SystemRoles.SuperAdmin)]
public class MenuManagementController(
    IMenuManagementService menuManagementService,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet("items")]
    public async Task<ActionResult<IReadOnlyCollection<MenuManagementItemDto>>> GetAllMenuItems(CancellationToken cancellationToken)
    {
        try
        {
            var items = await menuManagementService.GetAllMenuItemsAsync(cancellationToken);
            logger.Information("Retrieved {Count} navigation menu items", items.Count);
            return Ok(items);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving navigation menu items");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving menu items.");
        }
    }

    [HttpGet("items/{id:guid}")]
    public async Task<ActionResult<MenuManagementItemDto>> GetMenuItem(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await menuManagementService.GetMenuItemByIdAsync(id, cancellationToken);
            if (item is null)
            {
                logger.Warning("Navigation menu item {MenuItemId} not found", id);
                return NotFound();
            }
            return Ok(item);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving navigation menu item {MenuItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the menu item.");
        }
    }

    [HttpPut("items/{id:guid}/permissions")]
    public async Task<IActionResult> UpdateMenuPermissions(
        Guid id,
        [FromBody] UpdateMenuPermissionsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await menuManagementService.UpdateMenuPermissionsAsync(id, request.AllowedRoles, cancellationToken);
            logger.Information("Updated permissions for menu item {MenuItemId} with {RoleCount} roles", id, request.AllowedRoles.Count);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning(ex, "Menu item {MenuItemId} not found when updating permissions", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating permissions for menu item {MenuItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating menu permissions.");
        }
    }

    [HttpPut("items/{id:guid}/order")]
    public async Task<IActionResult> UpdateDisplayOrder(
        Guid id,
        [FromBody] ReorderMenuRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await menuManagementService.UpdateDisplayOrderAsync(id, request.DisplayOrder, cancellationToken);
            logger.Information("Updated display order for menu item {MenuItemId} to {DisplayOrder}", id, request.DisplayOrder);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning(ex, "Menu item {MenuItemId} not found when updating display order", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating display order for menu item {MenuItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating display order.");
        }
    }

    [HttpPut("items/{id:guid}/toggle")]
    public async Task<IActionResult> ToggleMenuItem(
        Guid id,
        [FromBody] bool isActive,
        CancellationToken cancellationToken)
    {
        try
        {
            await menuManagementService.ToggleMenuItemAsync(id, isActive, cancellationToken);
            logger.Information("Toggled menu item {MenuItemId} to {IsActive}", id, isActive);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning(ex, "Menu item {MenuItemId} not found when toggling", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error toggling menu item {MenuItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while toggling the menu item.");
        }
    }

    [HttpGet("items/{id:guid}/permissions")]
    public async Task<ActionResult<IReadOnlyCollection<string>>> GetMenuPermissions(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var item = await menuManagementService.GetMenuItemByIdAsync(id, cancellationToken);
            if (item is null)
            {
                logger.Warning("Navigation menu item {MenuItemId} not found when retrieving permissions", id);
                return NotFound();
            }
            return Ok(item.AllowedRoles);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving permissions for menu item {MenuItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving menu permissions.");
        }
    }
}

