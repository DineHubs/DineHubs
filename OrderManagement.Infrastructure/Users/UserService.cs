using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Users;
using OrderManagement.Identity.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Users;

public class UserService(
    UserManager<ApplicationUser> userManager,
    OrderManagementDbContext dbContext,
    IUsageTracker usageTracker,
    IPlanCatalog planCatalog) : IUserService
{
    public async Task<ApplicationUser> CreateUserAsync(Guid tenantId, string email, string password, string role, Guid? branchId, CancellationToken cancellationToken)
    {
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
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
        }

        var roleResult = await userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            // Cleanup user if role assignment fails
            await userManager.DeleteAsync(user);
            throw new InvalidOperationException(string.Join(", ", roleResult.Errors.Select(e => e.Description)));
        }

        return user;
    }

    public async Task<IReadOnlyCollection<UserDto>> GetUsersAsync(Guid tenantId, CancellationToken cancellationToken)
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

        return userDtos;
    }
}
