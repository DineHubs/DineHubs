using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Common;
using OrderManagement.Infrastructure.Persistence;
using OrderManagement.Infrastructure.Tenancy;

namespace OrderManagement.Tests.Tenancy;

public class TenantFilterTests
{
    [Fact]
    public async Task QueryFiltersRestrictResultsToTenant()
    {
        var tenantContext = new TenantContext();
        var currentUser = new TestCurrentUserContext();

        var options = new DbContextOptionsBuilder<OrderManagementDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        using var dbContext = new OrderManagementDbContext(options, tenantContext, currentUser, new DateTimeProvider());

        var tenantA = new Tenant("Tenant A", "tenanta");
        var tenantB = new Tenant("Tenant B", "tenantb");
        dbContext.Tenants.AddRange(tenantA, tenantB);
        await dbContext.SaveChangesAsync();

        dbContext.Branches.AddRange(
            new Branch(tenantA.Id, "A1", "A1", "Address 1", "KL"),
            new Branch(tenantB.Id, "B1", "B1", "Address 2", "Penang"));
        await dbContext.SaveChangesAsync();

        tenantContext.SetTenant(tenantA.Id, tenantA.Code);
        var branches = await dbContext.Branches.ToListAsync();

        Assert.Single(branches);
        Assert.Equal("A1", branches[0].Name);
    }

    private sealed class TestCurrentUserContext : ICurrentUserContext
    {
        public Guid? UserId => Guid.Empty;
        public string? Email => "tests@ordermanagement.local";
        public IReadOnlyCollection<string> Roles => Array.Empty<string>();
        public bool IsInRole(string role) => false;
    }
}


