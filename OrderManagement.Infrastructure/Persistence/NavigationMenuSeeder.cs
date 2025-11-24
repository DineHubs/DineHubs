using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Infrastructure.Persistence;

public static class NavigationMenuSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();

            // Get all tenants to seed menu for each
            var tenants = await dbContext.Tenants.ToListAsync(cancellationToken);

            foreach (var tenant in tenants)
            {
                try
                {
                    var existingMenu = await dbContext.NavigationMenuItems
                        .Where(m => m.TenantId == tenant.Id)
                        .AnyAsync(cancellationToken);

                    if (existingMenu)
                    {
                        Log.Information("Navigation menu already exists for tenant {TenantId}", tenant.Id);
                        continue;
                    }

                    await SeedMenuForTenantAsync(dbContext, tenant.Id, cancellationToken);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error seeding navigation menu for tenant {TenantId}", tenant.Id);
                    // Continue with next tenant
                }
            }

            Log.Information("Navigation menu seeding completed");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during navigation menu seeding");
            throw;
        }
    }

    private static async Task SeedMenuForTenantAsync(
        OrderManagementDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        // Create and save parent menu items first
        var dashboard = new NavigationMenuItem(tenantId, "Dashboard", "dashboard", "/dashboard", null, 1);
        dbContext.NavigationMenuItems.Add(dashboard);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, dashboard.Id, SystemRoles.All.ToArray(), cancellationToken);

        var orders = new NavigationMenuItem(tenantId, "Orders", "shopping_cart", "/orders", null, 2);
        dbContext.NavigationMenuItems.Add(orders);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, orders.Id, new[] { SystemRoles.Waiter, SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var menu = new NavigationMenuItem(tenantId, "Menu Management", "restaurant_menu", "/menu", null, 3);
        dbContext.NavigationMenuItems.Add(menu);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, menu.Id, new[] { SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin }, cancellationToken);

        var kitchen = new NavigationMenuItem(tenantId, "Kitchen", "restaurant", "/kitchen", null, 4);
        dbContext.NavigationMenuItems.Add(kitchen);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, kitchen.Id, new[] { SystemRoles.Kitchen, SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var inventory = new NavigationMenuItem(tenantId, "Inventory", "inventory_2", "/inventory", null, 5);
        dbContext.NavigationMenuItems.Add(inventory);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, inventory.Id, new[] { SystemRoles.InventoryManager, SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin, SystemRoles.Kitchen }, cancellationToken);

        var reports = new NavigationMenuItem(tenantId, "Reports", "assessment", "/reports", null, 6);
        dbContext.NavigationMenuItems.Add(reports);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, reports.Id, new[] { SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var subscriptions = new NavigationMenuItem(tenantId, "Subscriptions", "card_membership", "/subscriptions", null, 7);
        dbContext.NavigationMenuItems.Add(subscriptions);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, subscriptions.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var tenants = new NavigationMenuItem(tenantId, "Tenant Management", "business", "/tenants", null, 8);
        dbContext.NavigationMenuItems.Add(tenants);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, tenants.Id, new[] { SystemRoles.SuperAdmin }, cancellationToken);

        var users = new NavigationMenuItem(tenantId, "User Management", "people", "/users", null, 9);
        dbContext.NavigationMenuItems.Add(users);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, users.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var settings = new NavigationMenuItem(tenantId, "Settings", "settings", "/settings", null, 10);
        dbContext.NavigationMenuItems.Add(settings);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, settings.Id, SystemRoles.All.ToArray(), cancellationToken);

        // Now create child items
        var ordersActive = new NavigationMenuItem(tenantId, "Active Orders", "list", "/orders/active", orders.Id, 1);
        dbContext.NavigationMenuItems.Add(ordersActive);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, ordersActive.Id, new[] { SystemRoles.Waiter, SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var ordersHistory = new NavigationMenuItem(tenantId, "Order History", "history", "/orders/history", orders.Id, 2);
        dbContext.NavigationMenuItems.Add(ordersHistory);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, ordersHistory.Id, new[] { SystemRoles.Waiter, SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var ordersCreate = new NavigationMenuItem(tenantId, "Create Order", "add_shopping_cart", "/orders/create", orders.Id, 3);
        dbContext.NavigationMenuItems.Add(ordersCreate);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, ordersCreate.Id, new[] { SystemRoles.Waiter, SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var menuItemsView = new NavigationMenuItem(tenantId, "View Menu Items", "list", "/menu/items", menu.Id, 1);
        dbContext.NavigationMenuItems.Add(menuItemsView);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, menuItemsView.Id, new[] { SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin }, cancellationToken);

        var menuAdd = new NavigationMenuItem(tenantId, "Add Menu Item", "add", "/menu/add", menu.Id, 2);
        dbContext.NavigationMenuItems.Add(menuAdd);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, menuAdd.Id, new[] { SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin }, cancellationToken);

        var menuCategories = new NavigationMenuItem(tenantId, "Categories", "category", "/menu/categories", menu.Id, 3);
        dbContext.NavigationMenuItems.Add(menuCategories);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, menuCategories.Id, new[] { SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin }, cancellationToken);

        var kitchenQueue = new NavigationMenuItem(tenantId, "Order Queue", "queue", "/kitchen/queue", kitchen.Id, 1);
        dbContext.NavigationMenuItems.Add(kitchenQueue);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, kitchenQueue.Id, new[] { SystemRoles.Kitchen, SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var inventoryOverview = new NavigationMenuItem(tenantId, "Stock Overview", "view_list", "/inventory/overview", inventory.Id, 1);
        dbContext.NavigationMenuItems.Add(inventoryOverview);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, inventoryOverview.Id, new[] { SystemRoles.InventoryManager, SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin, SystemRoles.Kitchen }, cancellationToken);

        var inventoryAdd = new NavigationMenuItem(tenantId, "Add Stock", "add_box", "/inventory/add", inventory.Id, 2);
        dbContext.NavigationMenuItems.Add(inventoryAdd);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, inventoryAdd.Id, new[] { SystemRoles.InventoryManager, SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin }, cancellationToken);

        var inventorySuppliers = new NavigationMenuItem(tenantId, "Suppliers", "business", "/inventory/suppliers", inventory.Id, 3);
        dbContext.NavigationMenuItems.Add(inventorySuppliers);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, inventorySuppliers.Id, new[] { SystemRoles.InventoryManager, SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin }, cancellationToken);

        var inventoryReports = new NavigationMenuItem(tenantId, "Inventory Reports", "assessment", "/inventory/reports", inventory.Id, 4);
        dbContext.NavigationMenuItems.Add(inventoryReports);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, inventoryReports.Id, new[] { SystemRoles.InventoryManager, SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin }, cancellationToken);

        var reportsSales = new NavigationMenuItem(tenantId, "Sales Reports", "trending_up", "/reports/sales", reports.Id, 1);
        dbContext.NavigationMenuItems.Add(reportsSales);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, reportsSales.Id, new[] { SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var reportsCustomer = new NavigationMenuItem(tenantId, "Customer Reports", "people", "/reports/customers", reports.Id, 2);
        dbContext.NavigationMenuItems.Add(reportsCustomer);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, reportsCustomer.Id, new[] { SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var reportsWaiter = new NavigationMenuItem(tenantId, "Waiter Performance", "person", "/reports/waiters", reports.Id, 3);
        dbContext.NavigationMenuItems.Add(reportsWaiter);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, reportsWaiter.Id, new[] { SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var reportsInventory = new NavigationMenuItem(tenantId, "Inventory Reports", "inventory_2", "/reports/inventory", reports.Id, 4);
        dbContext.NavigationMenuItems.Add(reportsInventory);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, reportsInventory.Id, new[] { SystemRoles.Manager, SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var subscriptionsPlans = new NavigationMenuItem(tenantId, "Subscription Plans", "list", "/subscriptions/plans", subscriptions.Id, 1);
        dbContext.NavigationMenuItems.Add(subscriptionsPlans);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, subscriptionsPlans.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var subscriptionsUsage = new NavigationMenuItem(tenantId, "Usage Tracking", "analytics", "/subscriptions/usage", subscriptions.Id, 2);
        dbContext.NavigationMenuItems.Add(subscriptionsUsage);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, subscriptionsUsage.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var subscriptionsBilling = new NavigationMenuItem(tenantId, "Billing History", "receipt", "/subscriptions/billing", subscriptions.Id, 3);
        dbContext.NavigationMenuItems.Add(subscriptionsBilling);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, subscriptionsBilling.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var tenantsList = new NavigationMenuItem(tenantId, "All Tenants", "list", "/tenants", tenants.Id, 1);
        dbContext.NavigationMenuItems.Add(tenantsList);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, tenantsList.Id, new[] { SystemRoles.SuperAdmin }, cancellationToken);

        var tenantsCreate = new NavigationMenuItem(tenantId, "Create Tenant", "add_business", "/tenants/create", tenants.Id, 2);
        dbContext.NavigationMenuItems.Add(tenantsCreate);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, tenantsCreate.Id, new[] { SystemRoles.SuperAdmin }, cancellationToken);

        var tenantsSubscriptions = new NavigationMenuItem(tenantId, "Tenant Subscriptions", "card_membership", "/tenants/subscriptions", tenants.Id, 3);
        dbContext.NavigationMenuItems.Add(tenantsSubscriptions);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, tenantsSubscriptions.Id, new[] { SystemRoles.SuperAdmin }, cancellationToken);

        var usersList = new NavigationMenuItem(tenantId, "All Users", "list", "/users", users.Id, 1);
        dbContext.NavigationMenuItems.Add(usersList);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, usersList.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var usersCreate = new NavigationMenuItem(tenantId, "Create User", "person_add", "/users/create", users.Id, 2);
        dbContext.NavigationMenuItems.Add(usersCreate);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, usersCreate.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var usersRoles = new NavigationMenuItem(tenantId, "Manage Roles", "admin_panel_settings", "/users/roles", users.Id, 3);
        dbContext.NavigationMenuItems.Add(usersRoles);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, usersRoles.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var settingsProfile = new NavigationMenuItem(tenantId, "Profile", "person", "/settings/profile", settings.Id, 1);
        dbContext.NavigationMenuItems.Add(settingsProfile);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, settingsProfile.Id, SystemRoles.All.ToArray(), cancellationToken);

        var settingsBranch = new NavigationMenuItem(tenantId, "Branch Settings", "location_on", "/settings/branch", settings.Id, 2);
        dbContext.NavigationMenuItems.Add(settingsBranch);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, settingsBranch.Id, new[] { SystemRoles.Admin, SystemRoles.Manager, SystemRoles.SuperAdmin }, cancellationToken);

        var settingsTenant = new NavigationMenuItem(tenantId, "Tenant Settings", "business", "/settings/tenant", settings.Id, 3);
        dbContext.NavigationMenuItems.Add(settingsTenant);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, settingsTenant.Id, new[] { SystemRoles.Admin, SystemRoles.SuperAdmin }, cancellationToken);

        var settingsSystem = new NavigationMenuItem(tenantId, "System Settings", "admin_panel_settings", "/settings/system", settings.Id, 4);
        dbContext.NavigationMenuItems.Add(settingsSystem);
        await dbContext.SaveChangesAsync(cancellationToken);
        await AddPermissionsAsync(dbContext, settingsSystem.Id, new[] { SystemRoles.SuperAdmin }, cancellationToken);

        Log.Information("Seeded navigation menu for tenant {TenantId}", tenantId);
    }

    private static async Task AddPermissionsAsync(
        OrderManagementDbContext dbContext,
        Guid menuItemId,
        string[] roles,
        CancellationToken cancellationToken)
    {
        foreach (var role in roles)
        {
            var permission = new MenuPermission(menuItemId, role);
            dbContext.MenuPermissions.Add(permission);
        }
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
