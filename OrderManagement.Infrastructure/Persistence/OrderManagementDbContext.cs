using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Domain.Common;
using OrderManagement.Domain.Entities;
using OrderManagement.Identity.Entities;

namespace OrderManagement.Infrastructure.Persistence;

public class OrderManagementDbContext(
    DbContextOptions<OrderManagementDbContext> options,
    ITenantContext tenantContext,
    ICurrentUserContext currentUserContext,
    IDateTimeProvider dateTimeProvider)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    private readonly ITenantContext _tenantContext = tenantContext;
    private readonly ICurrentUserContext _currentUserContext = currentUserContext;
    private readonly IDateTimeProvider _dateTimeProvider = dateTimeProvider;

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<PaymentTransaction> Payments => Set<PaymentTransaction>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionPlan> Plans => Set<SubscriptionPlan>();
    public DbSet<PlanFeature> PlanFeatures => Set<PlanFeature>();
    public DbSet<BillingHistory> BillingHistories => Set<BillingHistory>();
    public DbSet<TenantUsageSnapshot> UsageSnapshots => Set<TenantUsageSnapshot>();
    public DbSet<QrOrderSession> QrSessions => Set<QrOrderSession>();
    public DbSet<BillingMessageTemplate> MessageTemplates => Set<BillingMessageTemplate>();
    public DbSet<NavigationMenuItem> NavigationMenuItems => Set<NavigationMenuItem>();
    public DbSet<MenuPermission> MenuPermissions => Set<MenuPermission>();
    public DbSet<EventLog> EventLogs => Set<EventLog>();
    public DbSet<ReceiptPrint> ReceiptPrints => Set<ReceiptPrint>();
    public DbSet<OrderMetrics> OrderMetrics => Set<OrderMetrics>();
    public DbSet<OrderException> OrderExceptions => Set<OrderException>();
    public DbSet<PrinterConfiguration> PrinterConfigurations => Set<PrinterConfiguration>();
    public DbSet<Table> Tables => Set<Table>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        ApplyTenantFilters(builder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = _dateTimeProvider.UtcNow;
                entry.Property(nameof(AuditableEntity.CreatedBy)).CurrentValue = _currentUserContext.UserId;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Property(nameof(AuditableEntity.UpdatedAt)).CurrentValue = _dateTimeProvider.UtcNow;
                entry.Property(nameof(AuditableEntity.UpdatedBy)).CurrentValue = _currentUserContext.UserId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTenantFilters(ModelBuilder builder)
    {
        var entityTypes = builder.Model.GetEntityTypes()
            .Where(t => typeof(TenantScopedEntity).IsAssignableFrom(t.ClrType));

        foreach (var entityType in entityTypes)
        {
            var method = typeof(OrderManagementDbContext)
                .GetMethod(nameof(SetTenantFilter), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(entityType.ClrType);

            method.Invoke(null, new object[] { builder, this });
        }
    }

    private static void SetTenantFilter<TEntity>(ModelBuilder builder, OrderManagementDbContext context)
        where TEntity : TenantScopedEntity
    {
        builder.Entity<TEntity>().HasQueryFilter(e => context._tenantContext.TenantId == Guid.Empty
            || e.TenantId == context._tenantContext.TenantId);
    }
}

