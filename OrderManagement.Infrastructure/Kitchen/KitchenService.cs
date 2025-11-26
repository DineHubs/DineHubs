using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Kitchen;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;
using Serilog;

namespace OrderManagement.Infrastructure.Kitchen;

public sealed class KitchenService(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : IKitchenService
{
    public async Task<IReadOnlyCollection<Order>> GetQueueAsync(CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.Orders
                .Where(o => o.TenantId == tenantContext.TenantId)
                .Where(o =>
                    o.Status == OrderStatus.Submitted ||
                    o.Status == OrderStatus.InPreparation ||
                    o.Status == OrderStatus.Ready);

            if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == tenantContext.BranchId);
            }

            // Load orders with tracking to ensure owned entities (Lines) are properly loaded
            // Then convert to list and detach to avoid tracking overhead
            var orders = await query.ToListAsync(cancellationToken);
            
            // Access Lines property to ensure they're materialized before detaching
            foreach (var order in orders)
            {
                _ = order.Lines.Count; // Force materialization of owned entities
            }
            
            // Detach all entities to avoid tracking
            dbContext.ChangeTracker.Clear();
            
            logger.Information("Retrieved {Count} orders from kitchen queue for tenant {TenantId}, branch {BranchId}", 
                orders.Count, tenantContext.TenantId, tenantContext.BranchId);
            
            return orders;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving kitchen queue for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);
            throw;
        }
    }
}

