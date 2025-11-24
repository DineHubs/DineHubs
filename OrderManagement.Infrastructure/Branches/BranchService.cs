using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Branches;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Branches;

public class BranchService(
    OrderManagementDbContext dbContext,
    IUsageTracker usageTracker,
    IPlanCatalog planCatalog) : IBranchService
{
    public async Task<Branch> CreateBranchAsync(Guid tenantId, string name, string location, CancellationToken cancellationToken)
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

        if (plan.MaxBranches > 0 && usage.ActiveBranches >= plan.MaxBranches)
        {
            throw new InvalidOperationException($"Branch limit reached for plan {plan.DisplayName}. Maximum allowed: {plan.MaxBranches}.");
        }

        // Generate branch code from name (simplified - in production, ensure uniqueness)
        var code = name.Replace(" ", "").ToUpperInvariant().Substring(0, Math.Min(10, name.Length));
        var branch = new Branch(tenantId, name, code, location, string.Empty);
        dbContext.Branches.Add(branch);
        await dbContext.SaveChangesAsync(cancellationToken);

        return branch;
    }

    public async Task<IReadOnlyCollection<Branch>> GetBranchesAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        return await dbContext.Branches
            .AsNoTracking()
            .Where(b => b.TenantId == tenantId)
            .ToListAsync(cancellationToken);
    }
}
