using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions.Billing;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Subscriptions;

public sealed class SubscriptionService(
    OrderManagementDbContext dbContext,
    IPlanCatalog planCatalog,
    Serilog.ILogger logger) : ISubscriptionService
{
    public async Task<Subscription> CreateAsync(Guid tenantId, SubscriptionPlanCode planCode, bool autoRenew, CancellationToken cancellationToken)
    {
        var plan = planCatalog.GetPlan(planCode);
        var subscription = new Subscription(tenantId, planCode, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1), autoRenew)
        {
            // default values already set
        };

        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
        return subscription;
    }

    public async Task ActivateAsync(Guid subscriptionId, string provider, string externalId, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.Subscriptions.FirstAsync(x => x.Id == subscriptionId, cancellationToken);
        subscription.Activate(provider, externalId);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RequestPlanChangeAsync(Guid tenantId, SubscriptionPlanCode newPlan, CancellationToken cancellationToken)
    {
        var subscription = await dbContext.Subscriptions.FirstAsync(x => x.TenantId == tenantId, cancellationToken);
        subscription.Suspend();
        subscription = new Subscription(tenantId, newPlan, DateTimeOffset.UtcNow, DateTimeOffset.UtcNow.AddMonths(1), subscription.AutoRenew);
        dbContext.Subscriptions.Add(subscription);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<BillingPayload?> BuildBillingPayloadAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.Tenants.FirstOrDefaultAsync(x => x.Id == tenantId, cancellationToken);
        var subscription = await dbContext.Subscriptions.FirstOrDefaultAsync(x => x.TenantId == tenantId, cancellationToken);
        if (tenant is null || subscription is null)
        {
            return null;
        }

        var plan = planCatalog.GetPlan(subscription.PlanCode);
        return new BillingPayload(
            tenant.Id,
            subscription.Id,
            plan.IncludesWhatsAppBilling ? "WhatsApp" : "Email",
            "billing@" + tenant.Code + ".com",
            plan.DisplayName,
            plan.MonthlyPrice,
            tenant.DefaultCurrency,
            subscription.StartDate,
            subscription.EndDate,
            null);
    }

    public async Task<IReadOnlyCollection<SubscriptionDto>> GetAllSubscriptionsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var subscriptions = await dbContext.Subscriptions
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(cancellationToken);

            var tenantIds = subscriptions.Select(s => s.TenantId).Distinct().ToList();
            var tenants = await dbContext.Tenants
                .AsNoTracking()
                .Where(t => tenantIds.Contains(t.Id))
                .ToDictionaryAsync(t => t.Id, t => t.Name, cancellationToken);

            var subscriptionDtos = subscriptions.Select(s =>
            {
                var plan = planCatalog.GetPlan(s.PlanCode);
                return new SubscriptionDto(
                    s.Id,
                    s.TenantId,
                    tenants.GetValueOrDefault(s.TenantId, "Unknown"),
                    s.PlanCode,
                    plan.DisplayName,
                    s.Status,
                    s.StartDate,
                    s.EndDate,
                    s.AutoRenew,
                    s.BillingProvider,
                    s.ExternalSubscriptionId);
            }).ToList();

            logger.Information("Retrieved {Count} subscriptions", subscriptionDtos.Count);
            return subscriptionDtos.AsReadOnly();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving all subscriptions");
            throw;
        }
    }

    public async Task<SubscriptionDto?> GetSubscriptionByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken)
    {
        try
        {
            var subscription = await dbContext.Subscriptions
                .AsNoTracking()
                .Where(s => s.TenantId == tenantId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (subscription == null)
            {
                return null;
            }

            var tenant = await dbContext.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId, cancellationToken);

            var plan = planCatalog.GetPlan(subscription.PlanCode);
            return new SubscriptionDto(
                subscription.Id,
                subscription.TenantId,
                tenant?.Name ?? "Unknown",
                subscription.PlanCode,
                plan.DisplayName,
                subscription.Status,
                subscription.StartDate,
                subscription.EndDate,
                subscription.AutoRenew,
                subscription.BillingProvider,
                subscription.ExternalSubscriptionId);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving subscription for tenant {TenantId}", tenantId);
            throw;
        }
    }
}


