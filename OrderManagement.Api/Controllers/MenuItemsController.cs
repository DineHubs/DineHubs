using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Api.Contracts.MenuItems;
using OrderManagement.Application.Abstractions;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Identity;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{SystemRoles.SuperAdmin},{SystemRoles.Admin},{SystemRoles.Manager}")]
public class MenuItemsController(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetMenuItems([FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.MenuItems
                .AsNoTracking()
                .Where(mi => mi.TenantId == tenantContext.TenantId);

            if (branchId.HasValue)
            {
                query = query.Where(mi => mi.BranchId == branchId.Value);
            }
            else if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(mi => mi.BranchId == tenantContext.BranchId.Value);
            }

            var items = await query
                .Select(mi => new MenuItemResponse(mi.Id, mi.BranchId, mi.Name, mi.Category, mi.Price, mi.IsAvailable, mi.ImageUrl))
                .ToListAsync(cancellationToken);

            logger.Information("Retrieved {Count} menu items for tenant {TenantId}, branch {BranchId}", 
                items.Count, tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return Ok(items);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving menu items for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving menu items.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<MenuItemResponse>> GetMenuItem(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var menuItem = await dbContext.MenuItems
                .AsNoTracking()
                .FirstOrDefaultAsync(mi => mi.Id == id && mi.TenantId == tenantContext.TenantId, cancellationToken);

            if (menuItem is null)
            {
                logger.Warning("Menu item {MenuItemId} not found for tenant {TenantId}", id, tenantContext.TenantId);
                return NotFound();
            }

            return Ok(new MenuItemResponse(menuItem.Id, menuItem.BranchId, menuItem.Name, menuItem.Category, menuItem.Price, menuItem.IsAvailable, menuItem.ImageUrl));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving menu item {MenuItemId} for tenant {TenantId}", id, tenantContext.TenantId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the menu item.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<MenuItemResponse>> CreateMenuItem([FromBody] CreateMenuItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var branch = await dbContext.Branches
                .FirstOrDefaultAsync(b => b.Id == request.BranchId && b.TenantId == tenantContext.TenantId, cancellationToken);

            if (branch is null)
            {
                logger.Warning("Invalid branch {BranchId} for tenant {TenantId} when creating menu item", request.BranchId, tenantContext.TenantId);
                return BadRequest("Invalid branch for current tenant.");
            }

            var menuItem = new MenuItem(tenantContext.TenantId, request.BranchId, request.Name, request.Category, request.Price, request.ImageUrl);
            if (!request.IsAvailable)
            {
                menuItem.ToggleAvailability(false);
            }

            dbContext.MenuItems.Add(menuItem);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Created menu item {Name} (Id: {MenuItemId}) for tenant {TenantId} branch {BranchId}", 
                menuItem.Name, menuItem.Id, tenantContext.TenantId, menuItem.BranchId);

            var response = new MenuItemResponse(menuItem.Id, menuItem.BranchId, menuItem.Name, menuItem.Category, menuItem.Price, menuItem.IsAvailable, menuItem.ImageUrl);
            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, response);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating menu item {Name} for tenant {TenantId} branch {BranchId}", 
                request.Name, tenantContext.TenantId, request.BranchId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the menu item.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] UpdateMenuItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var menuItem = await dbContext.MenuItems.FirstOrDefaultAsync(mi => mi.Id == id && mi.TenantId == tenantContext.TenantId, cancellationToken);
            if (menuItem is null)
            {
                logger.Warning("Menu item {MenuItemId} not found for tenant {TenantId} when updating", id, tenantContext.TenantId);
                return NotFound();
            }

            menuItem.UpdateDetails(request.Name, request.Category);
            menuItem.UpdatePrice(request.Price);
            menuItem.ToggleAvailability(request.IsAvailable);
            menuItem.UpdateImageUrl(request.ImageUrl);

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Updated menu item {MenuItemId} ({Name}) for tenant {TenantId}", id, request.Name, tenantContext.TenantId);
            return NoContent();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating menu item {MenuItemId} for tenant {TenantId}", id, tenantContext.TenantId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the menu item.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> DeleteMenuItem(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var menuItem = await dbContext.MenuItems.FirstOrDefaultAsync(mi => mi.Id == id && mi.TenantId == tenantContext.TenantId, cancellationToken);
            if (menuItem is null)
            {
                logger.Warning("Menu item {MenuItemId} not found for tenant {TenantId} when deleting", id, tenantContext.TenantId);
                return NotFound();
            }

            dbContext.MenuItems.Remove(menuItem);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Deleted menu item {MenuItemId} ({Name}) for tenant {TenantId}", id, menuItem.Name, tenantContext.TenantId);

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting menu item {MenuItemId} for tenant {TenantId}", id, tenantContext.TenantId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the menu item.");
        }
    }
}

