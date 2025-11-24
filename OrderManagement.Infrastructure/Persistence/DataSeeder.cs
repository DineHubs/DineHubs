using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Domain.Identity;
using OrderManagement.Identity.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Persistence;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Ensure database is created
            await dbContext.Database.EnsureCreatedAsync(cancellationToken);

            // Seed Tenant
            var tenant = await SeedTenantAsync(dbContext, cancellationToken);
            if (tenant is null) return;

            // Seed Branch
            var branch = await SeedBranchAsync(dbContext, tenant.Id, cancellationToken);
            if (branch is null) return;

            // Seed Users for all roles
            await SeedUsersAsync(userManager, tenant.Id, branch.Id, cancellationToken);

            // Seed Menu Items
            await SeedMenuItemsAsync(dbContext, tenant.Id, branch.Id, cancellationToken);

            Log.Information("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during data seeding");
            throw;
        }
    }

    private static async Task<Tenant?> SeedTenantAsync(
        OrderManagementDbContext dbContext,
        CancellationToken cancellationToken)
    {
        try
        {
            var existingTenant = await dbContext.Tenants
                .FirstOrDefaultAsync(t => t.Code == "dinehub", cancellationToken);

            if (existingTenant is not null)
            {
                Log.Information("Tenant 'dinehub' already exists");
                return existingTenant;
            }

            var tenant = new Tenant("DineHub Restaurant", "dinehub", "MY", "MYR");
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync(cancellationToken);
            Log.Information("Created tenant: {TenantName} ({Code})", tenant.Name, tenant.Code);
            return tenant;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error seeding tenant");
            throw;
        }
    }

    private static async Task<Branch?> SeedBranchAsync(
        OrderManagementDbContext dbContext,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            var existingBranch = await dbContext.Branches
                .FirstOrDefaultAsync(b => b.TenantId == tenantId && b.Code == "main", cancellationToken);

            if (existingBranch is not null)
            {
                Log.Information("Branch 'main' already exists for tenant {TenantId}", tenantId);
                return existingBranch;
            }

            var branch = new Branch(tenantId, "Main Branch", "main", "123 Jalan Bukit Bintang", "Kuala Lumpur");
            dbContext.Branches.Add(branch);
            await dbContext.SaveChangesAsync(cancellationToken);
            Log.Information("Created branch: {BranchName} ({Code}) for tenant {TenantId}", branch.Name, branch.Code, tenantId);
            return branch;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error seeding branch for tenant {TenantId}", tenantId);
            throw;
        }
    }

    private static async Task SeedUsersAsync(
        UserManager<ApplicationUser> userManager,
        Guid tenantId,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        var users = new[]
{
    new { Email = "superadmin@dinehub.com", Password = "SuperAdmin@123", Role = SystemRoles.SuperAdmin, DisplayName = "Super Admin", BranchId = (Guid?)null },
    new { Email = "admin@dinehub.com", Password = "Admin@123", Role = SystemRoles.Admin, DisplayName = "Admin User", BranchId = (Guid?)branchId },
    new { Email = "manager@dinehub.com", Password = "Manager@123", Role = SystemRoles.Manager, DisplayName = "Manager User", BranchId = (Guid?)branchId },
    new { Email = "waiter@dinehub.com", Password = "Waiter@123", Role = SystemRoles.Waiter, DisplayName = "Waiter User", BranchId = (Guid?)branchId },
    new { Email = "kitchen@dinehub.com", Password = "Kitchen@123", Role = SystemRoles.Kitchen, DisplayName = "Kitchen Staff", BranchId = (Guid?)branchId },
    new { Email = "inventory@dinehub.com", Password = "Inventory@123", Role = SystemRoles.InventoryManager, DisplayName = "Inventory Manager", BranchId = (Guid?)branchId }
};


        try
        {
            foreach (var userData in users)
            {
                try
                {
                    var existingUser = await userManager.FindByEmailAsync(userData.Email);
                    if (existingUser is not null)
                    {
                        Log.Information("User {Email} already exists", userData.Email);
                        continue;
                    }

                    var user = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = userData.Email,
                        Email = userData.Email,
                        EmailConfirmed = true,
                        TenantId = tenantId,
                        BranchId = userData.BranchId,
                        DisplayName = userData.DisplayName
                    };

                    var result = await userManager.CreateAsync(user, userData.Password);
                    if (!result.Succeeded)
                    {
                        Log.Error("Failed to create user {Email}: {Errors}",
                            userData.Email, string.Join(", ", result.Errors.Select(e => e.Description)));
                        continue;
                    }

                    await userManager.AddToRoleAsync(user, userData.Role);
                    Log.Information("Created user {Email} with role {Role}", userData.Email, userData.Role);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error creating user {Email}", userData.Email);
                    // Continue with next user
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error seeding users for tenant {TenantId}", tenantId);
            throw;
        }
    }

    private static async Task SeedMenuItemsAsync(
        OrderManagementDbContext dbContext,
        Guid tenantId,
        Guid branchId,
        CancellationToken cancellationToken)
    {
        try
        {
            var existingItems = await dbContext.MenuItems
                .Where(m => m.TenantId == tenantId && m.BranchId == branchId)
                .AnyAsync(cancellationToken);

            if (existingItems)
            {
                Log.Information("Menu items already exist for tenant {TenantId} branch {BranchId}", tenantId, branchId);
                return;
            }

        var menuItems = new[]
        {
            // Food Items
            new MenuItem(tenantId, branchId, "Dosa", "South Indian", 12.50m, "https://images.unsplash.com/photo-1612929633736-8c8b2f8b8b8b?w=800"),
            new MenuItem(tenantId, branchId, "Idli", "South Indian", 8.00m, "https://images.unsplash.com/photo-1585937421612-70a008356fbe?w=800"),
            new MenuItem(tenantId, branchId, "Vada", "South Indian", 6.50m, "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=800"),
            new MenuItem(tenantId, branchId, "Biryani Chicken", "Rice Dishes", 18.00m, "https://images.unsplash.com/photo-1589302168068-964664d93dc0?w=800"),
            new MenuItem(tenantId, branchId, "Biryani Mutton", "Rice Dishes", 22.00m, "https://images.unsplash.com/photo-1563379091339-03246963d96a?w=800"),
            new MenuItem(tenantId, branchId, "Nasi Lemak", "Malaysian", 10.00m, "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=800"),
            new MenuItem(tenantId, branchId, "Char Kway Teow", "Malaysian", 12.00m, "https://images.unsplash.com/photo-1551218808-94e220e084d2?w=800"),
            new MenuItem(tenantId, branchId, "Laksa", "Malaysian", 11.50m, "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=800"),
            new MenuItem(tenantId, branchId, "Roti Canai", "Malaysian", 4.50m, "https://images.unsplash.com/photo-1509440159596-0249088772ff?w=800"),
            new MenuItem(tenantId, branchId, "Chicken Curry", "Curries", 15.00m, "https://images.unsplash.com/photo-1588166524941-3bf61a9c41db?w=800"),
            new MenuItem(tenantId, branchId, "Fish Curry", "Curries", 16.00m, "https://images.unsplash.com/photo-1558030006-450675393462?w=800"),
            new MenuItem(tenantId, branchId, "Vegetable Curry", "Curries", 12.00m, "https://images.unsplash.com/photo-1588168333984-ffcbd7976619?w=800"),
            new MenuItem(tenantId, branchId, "Tandoori Chicken", "Grilled", 20.00m, "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800"),
            new MenuItem(tenantId, branchId, "Butter Chicken", "Curries", 18.50m, "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=800"),
            new MenuItem(tenantId, branchId, "Palak Paneer", "Vegetarian", 14.00m, "https://images.unsplash.com/photo-1588166524941-3bf61a9c41db?w=800"),
            new MenuItem(tenantId, branchId, "Dal Makhani", "Vegetarian", 13.00m, "https://images.unsplash.com/photo-1585937421612-70a008356fbe?w=800"),
            new MenuItem(tenantId, branchId, "Chicken Tikka", "Grilled", 19.00m, "https://images.unsplash.com/photo-1604503468506-a8da13d82791?w=800"),
            new MenuItem(tenantId, branchId, "Mutton Korma", "Curries", 21.00m, "https://images.unsplash.com/photo-1569718212165-3a8278d5f624?w=800"),
            new MenuItem(tenantId, branchId, "Fried Rice", "Rice Dishes", 11.00m, "https://images.unsplash.com/photo-1603133872878-684f208fb84b?w=800"),
            new MenuItem(tenantId, branchId, "Nasi Goreng", "Malaysian", 10.50m, "https://images.unsplash.com/photo-1551218808-94e220e084d2?w=800"),

            // Drinks
            new MenuItem(tenantId, branchId, "Teh Tarik", "Hot Beverages", 3.50m, "https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?w=800"),
            new MenuItem(tenantId, branchId, "Kopi O", "Hot Beverages", 3.00m, "https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?w=800"),
            new MenuItem(tenantId, branchId, "Milo", "Hot Beverages", 4.00m, "https://images.unsplash.com/photo-1517487881594-2787fef5ebf7?w=800"),
            new MenuItem(tenantId, branchId, "Lime Juice", "Cold Beverages", 4.50m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800"),
            new MenuItem(tenantId, branchId, "Orange Juice", "Cold Beverages", 5.00m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800"),
            new MenuItem(tenantId, branchId, "Watermelon Juice", "Cold Beverages", 5.50m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800"),
            new MenuItem(tenantId, branchId, "Mango Lassi", "Cold Beverages", 6.00m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800"),
            new MenuItem(tenantId, branchId, "Sweet Lassi", "Cold Beverages", 5.50m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800"),
            new MenuItem(tenantId, branchId, "Iced Lemon Tea", "Cold Beverages", 4.00m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800"),
            new MenuItem(tenantId, branchId, "Coca Cola", "Soft Drinks", 3.50m, "https://images.unsplash.com/photo-1554866585-cd94860890b7?w=800"),
            new MenuItem(tenantId, branchId, "Sprite", "Soft Drinks", 3.50m, "https://images.unsplash.com/photo-1554866585-cd94860890b7?w=800"),
            new MenuItem(tenantId, branchId, "Mineral Water", "Soft Drinks", 2.50m, "https://images.unsplash.com/photo-1554866585-cd94860890b7?w=800"),
            new MenuItem(tenantId, branchId, "Fresh Coconut", "Cold Beverages", 7.00m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800"),
            new MenuItem(tenantId, branchId, "Sugarcane Juice", "Cold Beverages", 5.00m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800"),
            new MenuItem(tenantId, branchId, "Bandung", "Cold Beverages", 4.50m, "https://images.unsplash.com/photo-1600271886742-f049cd451bba?w=800")
        };

            dbContext.MenuItems.AddRange(menuItems);
            await dbContext.SaveChangesAsync(cancellationToken);
            Log.Information("Seeded {Count} menu items for tenant {TenantId} branch {BranchId}", menuItems.Length, tenantId, branchId);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error seeding menu items for tenant {TenantId} branch {BranchId}", tenantId, branchId);
            throw;
        }
    }
}

