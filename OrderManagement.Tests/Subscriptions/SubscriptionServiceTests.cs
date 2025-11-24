using Microsoft.EntityFrameworkCore;
using System.Linq;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Application.Subscriptions.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Common;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Subscriptions;
using OrderManagement.Infrastructure.Tenancy;

namespace OrderManagement.Tests.Subscriptions;

public class SubscriptionServiceTests
{
    [Fact]
    public async Task CreatesSubscriptionWithSelectedPlan()
    {
        var tenantContext = new TenantContext();
        var currentUser = new TestCurrentUserContext();
        var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        using var dbContext = new OrderManagementDbContext(options, tenantContext, currentUser, new DateTimeProvider());
        var catalog = new InMemoryPlanCatalog();
        var service = new SubscriptionService(dbContext, catalog);

        var tenant = new Tenant("Test", "test");
        dbContext.Tenants.Add(tenant);
        await dbContext.SaveChangesAsync();

        var subscription = await service.CreateAsync(tenant.Id, SubscriptionPlanCode.Basic, autoRenew: true, CancellationToken.None);

        Assert.Equal(SubscriptionPlanCode.Basic, subscription.PlanCode);
        Assert.Equal(SubscriptionStatus.Pending, subscription.Status);
    }

    private sealed class InMemoryPlanCatalog : IPlanCatalog
    {
        private readonly IReadOnlyCollection<SubscriptionPlanDto> _plans =
        [
            new SubscriptionPlanDto(
                SubscriptionPlanCode.Basic,
                "Basic",
                100,
                1000,
                5,
                20,
                true,
                false,
                false)
        ];

        public SubscriptionPlanDto GetPlan(SubscriptionPlanCode code) => _plans.First();
        public IReadOnlyCollection<SubscriptionPlanDto> GetPlans() => _plans;
    }

    private sealed class TestCurrentUserContext : ICurrentUserContext
    {
        public Guid? UserId => Guid.Empty;
        public string? Email => "tests@ordermanagement.local";
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public bool IsInRole(string role) => false;
    }
}

