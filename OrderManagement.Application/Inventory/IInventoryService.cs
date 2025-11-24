using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Inventory;

public interface IInventoryService
{
    Task<InventoryItem> CreateItemAsync(Guid tenantId, string name, string uom, decimal reorderPoint, CancellationToken cancellationToken);
    Task RecordMovementAsync(Guid itemId, decimal quantity, string reference, CancellationToken cancellationToken);
}


