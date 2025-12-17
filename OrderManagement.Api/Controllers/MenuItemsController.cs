using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OrderManagement.Api.Contracts.MenuItems;
using OrderManagement.Application.MenuItems;
using OrderManagement.Application.MenuItems.Models;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class MenuItemsController(
    IMenuItemService menuItemService,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetMenuItems([FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var items = await menuItemService.GetMenuItemsAsync(branchId, cancellationToken);
            var response = items.Select(i => new MenuItemResponse(i.Id, i.BranchId, i.Name, i.Category, i.Price, i.IsAvailable, i.ImageUrl));
            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving menu items");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving menu items.");
        }
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<ActionResult<MenuItemResponse>> GetMenuItem(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var menuItem = await menuItemService.GetMenuItemByIdAsync(id, cancellationToken);
            if (menuItem is null)
            {
                return NotFound();
            }

            return Ok(new MenuItemResponse(menuItem.Id, menuItem.BranchId, menuItem.Name, menuItem.Category, menuItem.Price, menuItem.IsAvailable, menuItem.ImageUrl));
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving menu item {MenuItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the menu item.");
        }
    }

    [HttpPost]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<ActionResult<MenuItemResponse>> CreateMenuItem([FromBody] CreateMenuItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = new CreateMenuItemDto(request.BranchId, request.Name, request.Category, request.Price, request.IsAvailable, request.ImageUrl);
            var menuItem = await menuItemService.CreateMenuItemAsync(dto, cancellationToken);
            var response = new MenuItemResponse(menuItem.Id, menuItem.BranchId, menuItem.Name, menuItem.Category, menuItem.Price, menuItem.IsAvailable, menuItem.ImageUrl);
            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error creating menu item: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating menu item");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the menu item.");
        }
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<IActionResult> UpdateMenuItem(Guid id, [FromBody] UpdateMenuItemRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var dto = new UpdateMenuItemDto(request.Name, request.Category, request.Price, request.IsAvailable, request.ImageUrl);
            await menuItemService.UpdateMenuItemAsync(id, dto, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error updating menu item {MenuItemId}: {Message}", id, ex.Message);
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating menu item {MenuItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the menu item.");
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<IActionResult> DeleteMenuItem(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await menuItemService.DeleteMenuItemAsync(id, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error deleting menu item {MenuItemId}: {Message}", id, ex.Message);
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting menu item {MenuItemId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the menu item.");
        }
    }
}

