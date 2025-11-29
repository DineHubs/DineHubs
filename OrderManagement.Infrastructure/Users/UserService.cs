using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Users;
using OrderManagement.Domain.Identity;
using OrderManagement.Identity.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Users;

public class UserService(
    UserManager<ApplicationUser> userManager,
    OrderManagementDbContext dbContext,
    IUsageTracker usageTracker,
    IPlanCatalog planCatalog,
    Serilog.ILogger logger) : IUserService
{
    private static readonly string[] AdminAllowedRoles = 
    [
        SystemRoles.Manager,
        SystemRoles.Waiter,
        SystemRoles.Kitchen,
        SystemRoles.InventoryManager
    ];

    public void ValidateUserCreationRole(string requestedRole, IReadOnlyCollection<string> currentUserRoles)
    {
        // Validate requested role is not null or empty
        if (string.IsNullOrWhiteSpace(requestedRole))
        {
            throw new InvalidOperationException("Role is required.");
        }

        // Validate requested role is a valid system role
        if (!SystemRoles.All.Contains(requestedRole, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Invalid role '{requestedRole}'. Valid roles are: {string.Join(", ", SystemRoles.All)}");
        }

        // Check if current user is SuperAdmin - SuperAdmin can create any role
        var isSuperAdmin = currentUserRoles.Contains(SystemRoles.SuperAdmin, StringComparer.OrdinalIgnoreCase);
        if (isSuperAdmin)
        {
            logger.Information("SuperAdmin creating user with role {Role}", requestedRole);
            return; // SuperAdmin can create any role
        }

        // Check if current user is Admin (not SuperAdmin) - Admin has restrictions
        var isAdmin = currentUserRoles.Contains(SystemRoles.Admin, StringComparer.OrdinalIgnoreCase);
        if (isAdmin)
        {
            if (!AdminAllowedRoles.Contains(requestedRole, StringComparer.OrdinalIgnoreCase))
            {
                logger.Warning("Admin user attempted to create user with restricted role {Role}. Allowed roles: {AllowedRoles}", 
                    requestedRole, string.Join(", ", AdminAllowedRoles));
                throw new InvalidOperationException(
                    $"Admin users can only create users with roles: {string.Join(", ", AdminAllowedRoles)}");
            }
            logger.Information("Admin creating user with role {Role}", requestedRole);
            return;
        }

        // If user is neither SuperAdmin nor Admin, they shouldn't be able to create users
        // This should be caught by authorization, but we'll validate here too
        logger.Warning("User with roles {Roles} attempted to create user. Only SuperAdmin and Admin can create users.", 
            string.Join(", ", currentUserRoles));
        throw new InvalidOperationException("Only SuperAdmin and Admin users can create new users.");
    }

    public async Task<ApplicationUser> CreateUserAsync(Guid tenantId, string email, string password, string role, Guid? branchId, IReadOnlyCollection<string> currentUserRoles, CancellationToken cancellationToken)
    {
        try
        {
            // Validate role based on current user's roles
            ValidateUserCreationRole(role, currentUserRoles);

            // Check subscription limits
            var subscription = await dbContext.Subscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.TenantId == tenantId, cancellationToken);

            if (subscription is null)
            {
                throw new InvalidOperationException("No active subscription found.");
            }

            var plan = planCatalog.GetPlan(subscription.PlanCode);
            var usage = await usageTracker.CaptureAsync(tenantId, cancellationToken);

            if (plan.MaxUsers > 0 && usage.ActiveUsers >= plan.MaxUsers)
            {
                throw new InvalidOperationException($"User limit reached for plan {plan.DisplayName}. Maximum allowed: {plan.MaxUsers}.");
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                TenantId = tenantId,
                BranchId = branchId,
                DisplayName = email.Split('@')[0]
            };

            var result = await userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                logger.Error("Failed to create user {Email}: {Errors}", email, errors);
                throw new InvalidOperationException($"Failed to create user: {errors}");
            }

            var roleResult = await userManager.AddToRoleAsync(user, role);
            if (!roleResult.Succeeded)
            {
                // Cleanup user if role assignment fails
                await userManager.DeleteAsync(user);
                var errors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                logger.Error("Failed to assign role {Role} to user {Email}: {Errors}", role, email, errors);
                throw new InvalidOperationException($"Failed to assign role: {errors}");
            }

            logger.Information("Successfully created user {Email} with role {Role} for tenant {TenantId}", email, role, tenantId);
            return user;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error creating user {Email} for tenant {TenantId}", email, tenantId);
            throw new InvalidOperationException("An error occurred while creating the user.");
        }
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var users = await userManager.Users
                .AsNoTracking()
                .Where(u => u.TenantId == tenantId)
                .ToListAsync(cancellationToken);

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userDtos.Add(new UserDto(user.Id, user.Email!, roles.FirstOrDefault() ?? "None", user.BranchId, user.IsActive));
            }

            logger.Information("Retrieved {Count} users for tenant {TenantId}", userDtos.Count, tenantId);
            return userDtos;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving users for tenant {TenantId}", tenantId);
            throw new InvalidOperationException("An error occurred while retrieving users.");
        }
    }
}
