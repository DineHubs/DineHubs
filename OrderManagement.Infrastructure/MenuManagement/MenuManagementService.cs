using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.MenuManagement;
using OrderManagement.Application.MenuManagement.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.MenuManagement;

public sealed class MenuManagementService(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : IMenuManagementService
{
    public async Task<IReadOnlyCollection<MenuManagementItemDto>> GetAllMenuItemsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var menuItems = await dbContext.NavigationMenuItems
                .Where(m => m.TenantId == tenantContext.TenantId)
                .Include(m => m.Permissions)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync(cancellationToken);

            var result = menuItems.Select(m => new MenuManagementItemDto(
                m.Id,
                m.Label,
                m.Icon,
                m.Route,
                m.ParentId,
                m.DisplayOrder,
                m.IsActive,
                m.Permissions.Select(p => p.RoleName).ToList().AsReadOnly()
            )).ToList().AsReadOnly();

            logger.Information("Retrieved {Count} navigation menu items for tenant {TenantId}", result.Count, tenantContext.TenantId);
            return result;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving navigation menu items for tenant {TenantId}", tenantContext.TenantId);
            throw;
        }
    }

    public async Task<MenuManagementItemDto?> GetMenuItemByIdAsync(Guid menuItemId, CancellationToken cancellationToken = default)
    {
        try
        {
            var menuItem = await dbContext.NavigationMenuItems
                .Where(m => m.Id == menuItemId && m.TenantId == tenantContext.TenantId)
                .Include(m => m.Permissions)
                .FirstOrDefaultAsync(cancellationToken);

            if (menuItem is null)
            {
                logger.Warning("Navigation menu item {MenuItemId} not found for tenant {TenantId}", menuItemId, tenantContext.TenantId);
                return null;
            }

            return new MenuManagementItemDto(
                menuItem.Id,
                menuItem.Label,
                menuItem.Icon,
                menuItem.Route,
                menuItem.ParentId,
                menuItem.DisplayOrder,
                menuItem.IsActive,
                menuItem.Permissions.Select(p => p.RoleName).ToList().AsReadOnly()
            );
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving navigation menu item {MenuItemId} for tenant {TenantId}", menuItemId, tenantContext.TenantId);
            throw;
        }
    }

    public async Task UpdateMenuPermissionsAsync(Guid menuItemId, IReadOnlyCollection<string> allowedRoles, CancellationToken cancellationToken = default)
    {
        try
        {
            var menuItem = await dbContext.NavigationMenuItems
                .Where(m => m.Id == menuItemId && m.TenantId == tenantContext.TenantId)
                .Include(m => m.Permissions)
                .FirstOrDefaultAsync(cancellationToken);

            if (menuItem is null)
            {
                logger.Warning("Menu item {MenuItemId} not found for tenant {TenantId} when updating permissions", menuItemId, tenantContext.TenantId);
                throw new InvalidOperationException($"Menu item with ID {menuItemId} not found.");
            }

            // Remove existing permissions
            var permissionsToRemove = menuItem.Permissions
                .Where(p => !allowedRoles.Contains(p.RoleName, StringComparer.OrdinalIgnoreCase))
                .ToList();

            foreach (var permission in permissionsToRemove)
            {
                dbContext.MenuPermissions.Remove(permission);
            }

            // Add new permissions
            var existingRoleNames = menuItem.Permissions
                .Select(p => p.RoleName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var roleName in allowedRoles)
            {
                if (!existingRoleNames.Contains(roleName))
                {
                    var permission = new MenuPermission(menuItemId, roleName);
                    dbContext.MenuPermissions.Add(permission);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Updated permissions for menu item {MenuItemId} (tenant {TenantId}) with {RoleCount} roles", 
                menuItemId, tenantContext.TenantId, allowedRoles.Count);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating permissions for menu item {MenuItemId} (tenant {TenantId})", menuItemId, tenantContext.TenantId);
            throw;
        }
    }

    public async Task UpdateDisplayOrderAsync(Guid menuItemId, int displayOrder, CancellationToken cancellationToken = default)
    {
        try
        {
            var menuItem = await dbContext.NavigationMenuItems
                .Where(m => m.Id == menuItemId && m.TenantId == tenantContext.TenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (menuItem is null)
            {
                logger.Warning("Menu item {MenuItemId} not found for tenant {TenantId} when updating display order", menuItemId, tenantContext.TenantId);
                throw new InvalidOperationException($"Menu item with ID {menuItemId} not found.");
            }

            menuItem.UpdateDisplayOrder(displayOrder);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Updated display order for menu item {MenuItemId} (tenant {TenantId}) to {DisplayOrder}", 
                menuItemId, tenantContext.TenantId, displayOrder);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating display order for menu item {MenuItemId} (tenant {TenantId})", menuItemId, tenantContext.TenantId);
            throw;
        }
    }

    public async Task ToggleMenuItemAsync(Guid menuItemId, bool isActive, CancellationToken cancellationToken = default)
    {
        try
        {
            var menuItem = await dbContext.NavigationMenuItems
                .Where(m => m.Id == menuItemId && m.TenantId == tenantContext.TenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (menuItem is null)
            {
                logger.Warning("Menu item {MenuItemId} not found for tenant {TenantId} when toggling", menuItemId, tenantContext.TenantId);
                throw new InvalidOperationException($"Menu item with ID {menuItemId} not found.");
            }

            menuItem.ToggleActive(isActive);
            await dbContext.SaveChangesAsync(cancellationToken);
            logger.Information("Toggled menu item {MenuItemId} (tenant {TenantId}) to {IsActive}", 
                menuItemId, tenantContext.TenantId, isActive);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error toggling menu item {MenuItemId} (tenant {TenantId})", menuItemId, tenantContext.TenantId);
            throw;
        }
    }
}

