using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class InventoryItem : TenantScopedEntity
{
    public string Name { get; private set; } = string.Empty;
    public string UnitOfMeasure { get; private set; } = "pcs";
    public decimal QuantityOnHand { get; private set; }
    public decimal ReorderPoint { get; private set; }
    public string SupplierName { get; private set; } = string.Empty;

    private readonly List<InventoryMovement> _movements = new();
    public IReadOnlyCollection<InventoryMovement> Movements => _movements;

    private InventoryItem()
    {
    }

    public InventoryItem(Guid tenantId, string name, string unitOfMeasure, decimal reorderPoint)
        : base(tenantId)
    {
        Name = name;
        UnitOfMeasure = unitOfMeasure;
        ReorderPoint = reorderPoint;
    }

    public void RecordMovement(decimal quantity, InventoryEventType type, string reference)
    {
        QuantityOnHand += quantity;
        _movements.Add(new InventoryMovement(quantity, type, reference));
    }
}

public class InventoryMovement
{
    public decimal Quantity { get; }
    public InventoryEventType EventType { get; }
    public string Reference { get; } = string.Empty;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;

    private InventoryMovement()
    {
    }

    public InventoryMovement(decimal quantity, InventoryEventType eventType, string reference)
    {
        Quantity = quantity;
        EventType = eventType;
        Reference = reference;
    }
}

