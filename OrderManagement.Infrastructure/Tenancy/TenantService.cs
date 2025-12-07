using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Tenants;
using OrderManagement.Application.Tenants.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Tenancy;

public sealed class TenantService(
    OrderManagementDbContext dbContext,
    Serilog.ILogger logger) : ITenantService
{
    public async Task<IReadOnlyCollection<TenantDto>> GetAllTenantsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var tenants = await dbContext.Tenants
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            // Get all subscriptions for these tenants
            var tenantIds = tenants.Select(t => t.Id).ToList();
            var subscriptions = await dbContext.Subscriptions
                .AsNoTracking()
                .Where(s => tenantIds.Contains(s.TenantId))
                .ToListAsync(cancellationToken);

            var subscriptionsByTenant = subscriptions
                .GroupBy(s => s.TenantId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(s => s.CreatedAt).FirstOrDefault());

            var tenantDtos = tenants.Select(t =>
            {
                var subscription = subscriptionsByTenant.GetValueOrDefault(t.Id);
                return new TenantDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    Code = t.Code,
                    CountryCode = t.CountryCode,
                    DefaultCurrency = t.DefaultCurrency,
                    IsActive = t.IsActive,
                    CreatedAt = t.CreatedAt,
                    SubscriptionStatus = subscription?.Status.ToString(),
                    SubscriptionPlanCode = subscription?.PlanCode.ToString(),
                    SubscriptionStartDate = subscription?.StartDate,
                    SubscriptionEndDate = subscription?.EndDate,
                    SubscriptionAutoRenew = subscription?.AutoRenew
                };
            }).ToList();

            logger.Information("Retrieved {Count} tenants with subscription data", tenantDtos.Count);
            return tenantDtos;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving tenants");
            throw new InvalidOperationException("An error occurred while retrieving tenants.");
        }
    }

    public async Task<TenantDetailDto?> GetTenantByIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

            if (tenant == null)
            {
                return null;
            }

            var subscription = await dbContext.Subscriptions
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var branchesCount = await dbContext.Branches
                .AsNoTracking()
                .CountAsync(b => b.TenantId == tenantId, cancellationToken);

            var usersCount = await dbContext.Users
                .AsNoTracking()
                .CountAsync(u => u.TenantId == tenantId, cancellationToken);

            var tenantDetail = new TenantDetailDto
            {
                Id = tenant.Id,
                Name = tenant.Name,
                Code = tenant.Code,
                CountryCode = tenant.CountryCode,
                DefaultCurrency = tenant.DefaultCurrency,
                IsActive = tenant.IsActive,
                CreatedAt = tenant.CreatedAt,
                SubscriptionStatus = subscription?.Status.ToString(),
                SubscriptionPlanCode = subscription?.PlanCode.ToString(),
                SubscriptionStartDate = subscription?.StartDate,
                SubscriptionEndDate = subscription?.EndDate,
                SubscriptionAutoRenew = subscription?.AutoRenew,
                BranchesCount = branchesCount,
                UsersCount = usersCount,
                Subscription = subscription != null ? new SubscriptionDetailDto
                {
                    Id = subscription.Id,
                    Status = subscription.Status.ToString(),
                    PlanCode = subscription.PlanCode.ToString(),
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    AutoRenew = subscription.AutoRenew,
                    BillingProvider = subscription.BillingProvider,
                    ExternalSubscriptionId = subscription.ExternalSubscriptionId
                } : null
            };

            logger.Information("Retrieved tenant details for {TenantId}", tenantId);
            return tenantDetail;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving tenant details for {TenantId}", tenantId);
            throw new InvalidOperationException("An error occurred while retrieving tenant details.");
        }
    }
}

