using OrderManagement.Application.Inventory;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Inventory;

public sealed class InventoryService(OrderManagementDbContext dbContext) : IInventoryService
{
    public async Task<InventoryItem> CreateItemAsync(Guid tenantId, string name, string uom, decimal reorderPoint, CancellationToken cancellationToken)
    {
        var item = new InventoryItem(tenantId, name, uom, reorderPoint);
        dbContext.InventoryItems.Add(item);
        await dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task RecordMovementAsync(Guid itemId, decimal quantity, string reference, CancellationToken cancellationToken)
    {
        var item = await dbContext.InventoryItems.FindAsync([itemId], cancellationToken);
        if (item is null)
        {
            throw new InvalidOperationException("Inventory item not found");
        }

        item.RecordMovement(quantity, InventoryEventType.Adjustment, reference);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}


