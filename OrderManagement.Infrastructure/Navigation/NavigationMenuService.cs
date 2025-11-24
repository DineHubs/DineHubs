using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Navigation;
using ApplicationMenuItem = OrderManagement.Application.Navigation.Models.NavigationMenuItem;
using DomainMenuItem = OrderManagement.Domain.Entities.NavigationMenuItem;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Navigation;

public sealed class NavigationMenuService(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : INavigationMenuService
{
    public async Task<IReadOnlyCollection<ApplicationMenuItem>> GetMenuForRolesAsync(IEnumerable<string> roles, CancellationToken cancellationToken = default)
    {
        try
        {
            var userRoles = roles.ToHashSet(StringComparer.OrdinalIgnoreCase);

            // Load all active menu items for the tenant with their permissions
            var menuItems = await dbContext.NavigationMenuItems
                .Where(m => m.TenantId == tenantContext.TenantId && m.IsActive)
                .Include(m => m.Permissions)
                .OrderBy(m => m.DisplayOrder)
                .ToListAsync(cancellationToken);

            // Build menu tree and filter by roles
            var menuTree = BuildMenuTree(menuItems);
            var filteredMenu = FilterMenuByRoles(menuTree, userRoles);

            logger.Information("Retrieved navigation menu for tenant {TenantId} with {RoleCount} roles, {MenuCount} items", 
                tenantContext.TenantId, userRoles.Count, filteredMenu.Count);
            return filteredMenu;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving navigation menu for tenant {TenantId}", tenantContext.TenantId);
            throw;
        }
    }

    private static List<ApplicationMenuItem> BuildMenuTree(List<DomainMenuItem> menuItems)
    {
        var rootItems = menuItems
            .Where(m => m.ParentId is null)
            .Select(m => ConvertToNavigationMenuItem(m, menuItems))
            .ToList();

        return rootItems;
    }

    private static ApplicationMenuItem ConvertToNavigationMenuItem(
        DomainMenuItem item,
        List<DomainMenuItem> allItems)
    {
        var allowedRoles = item.Permissions.Select(p => p.RoleName).ToList();
        var children = allItems
            .Where(m => m.ParentId == item.Id)
            .Select(m => ConvertToNavigationMenuItem(m, allItems))
            .ToList();

        return new ApplicationMenuItem(
            item.Id.ToString(),
            item.Label,
            item.Icon,
            item.Route,
            item.ParentId?.ToString(),
            allowedRoles,
            children.Any() ? children : null
        );
    }

    private static List<ApplicationMenuItem> FilterMenuByRoles(List<ApplicationMenuItem> menuItems, HashSet<string> userRoles)
    {
        var filtered = new List<ApplicationMenuItem>();

        foreach (var item in menuItems)
        {
            // Check if user has access to this menu item
            if (!HasAccess(item.AllowedRoles, userRoles))
                continue;

            // Filter children if they exist
            ApplicationMenuItem? filteredItem = null;
            if (item.Children is not null && item.Children.Any())
            {
                var filteredChildren = FilterMenuByRoles(item.Children.ToList(), userRoles);
                if (filteredChildren.Any())
                {
                    filteredItem = item with { Children = filteredChildren };
                }
                else if (item.Route is not null)
                {
                    // If no children are accessible but item has a route, include it
                    filteredItem = item with { Children = null };
                }
            }
            else
            {
                filteredItem = item;
            }

            if (filteredItem is not null)
            {
                filtered.Add(filteredItem);
            }
        }

        return filtered;
    }

    private static bool HasAccess(IReadOnlyCollection<string> allowedRoles, HashSet<string> userRoles)
    {
        return allowedRoles.Any(role => userRoles.Contains(role, StringComparer.OrdinalIgnoreCase));
    }
}
