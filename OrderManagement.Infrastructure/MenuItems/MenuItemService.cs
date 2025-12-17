using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.MenuItems;
using OrderManagement.Application.MenuItems.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.MenuItems;

public sealed class MenuItemService(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : IMenuItemService
{
    public async Task<IReadOnlyCollection<MenuItemDto>> GetMenuItemsAsync(Guid? branchId, CancellationToken cancellationToken = default)
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
                .Select(mi => new MenuItemDto(mi.Id, mi.BranchId, mi.Name, mi.Category, mi.Price, mi.IsAvailable, mi.ImageUrl))
                .ToListAsync(cancellationToken);

            logger.Information("Retrieved {Count} menu items for tenant {TenantId}, branch {BranchId}",
                items.Count, tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            
            return items.AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving menu items for tenant {TenantId}, branch {BranchId}",
                tenantContext.TenantId, branchId ?? tenantContext.BranchId);
            throw;
        }
    }

    public async Task<MenuItemDto?> GetMenuItemByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var menuItem = await dbContext.MenuItems
                .AsNoTracking()
                .FirstOrDefaultAsync(mi => mi.Id == id && mi.TenantId == tenantContext.TenantId, cancellationToken);

            if (menuItem is null)
            {
                logger.Warning("Menu item {MenuItemId} not found for tenant {TenantId}", id, tenantContext.TenantId);
                return null;
            }

            return new MenuItemDto(menuItem.Id, menuItem.BranchId, menuItem.Name, menuItem.Category, menuItem.Price, menuItem.IsAvailable, menuItem.ImageUrl);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving menu item {MenuItemId} for tenant {TenantId}", id, tenantContext.TenantId);
            throw;
        }
    }

    public async Task<MenuItemDto> CreateMenuItemAsync(CreateMenuItemDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            // Verify branch belongs to tenant
            var branch = await dbContext.Branches
                .FirstOrDefaultAsync(b => b.Id == dto.BranchId && b.TenantId == tenantContext.TenantId, cancellationToken);

            if (branch is null)
            {
                throw new InvalidOperationException("Invalid branch for current tenant.");
            }

            var menuItem = new MenuItem(tenantContext.TenantId, dto.BranchId, dto.Name, dto.Category, dto.Price, dto.ImageUrl);
            if (!dto.IsAvailable)
            {
                menuItem.ToggleAvailability(false);
            }

            dbContext.MenuItems.Add(menuItem);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.Information("Created menu item {Name} (Id: {MenuItemId}) for tenant {TenantId} branch {BranchId}",
                menuItem.Name, menuItem.Id, tenantContext.TenantId, menuItem.BranchId);

            return new MenuItemDto(menuItem.Id, menuItem.BranchId, menuItem.Name, menuItem.Category, menuItem.Price, menuItem.IsAvailable, menuItem.ImageUrl);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating menu item {Name} for tenant {TenantId} branch {BranchId}",
                dto.Name, tenantContext.TenantId, dto.BranchId);
            throw;
        }
    }

    public async Task UpdateMenuItemAsync(Guid id, UpdateMenuItemDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var menuItem = await dbContext.MenuItems.FirstOrDefaultAsync(mi => mi.Id == id && mi.TenantId == tenantContext.TenantId, cancellationToken);
            if (menuItem is null)
            {
                throw new InvalidOperationException($"Menu item {id} not found for current tenant.");
            }

            menuItem.UpdateDetails(dto.Name, dto.Category);
            menuItem.UpdatePrice(dto.Price);
            menuItem.ToggleAvailability(dto.IsAvailable);
            menuItem.UpdateImageUrl(dto.ImageUrl);

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Updated menu item {MenuItemId} ({Name}) for tenant {TenantId}", id, dto.Name, tenantContext.TenantId);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating menu item {MenuItemId} for tenant {TenantId}", id, tenantContext.TenantId);
            throw;
        }
    }

    public async Task DeleteMenuItemAsync(Guid id, CancellationToken cancellationToken = default)
    {
        try
        {
            var menuItem = await dbContext.MenuItems.FirstOrDefaultAsync(mi => mi.Id == id && mi.TenantId == tenantContext.TenantId, cancellationToken);
            if (menuItem is null)
            {
                throw new InvalidOperationException($"Menu item {id} not found for current tenant.");
            }

            dbContext.MenuItems.Remove(menuItem);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Deleted menu item {MenuItemId} ({Name}) for tenant {TenantId}", id, menuItem.Name, tenantContext.TenantId);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting menu item {MenuItemId} for tenant {TenantId}", id, tenantContext.TenantId);
            throw;
        }
    }
}

