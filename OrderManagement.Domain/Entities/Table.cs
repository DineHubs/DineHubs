using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class Table : BranchScopedEntity
{
    public string TableNumber { get; private set; } = string.Empty;
    public TableStatus Status { get; private set; } = TableStatus.Available;
    public double PositionX { get; private set; }
    public double PositionY { get; private set; }
    public double Width { get; private set; } = 100;
    public double Height { get; private set; } = 100;

    private Table()
    {
    }

    public Table(Guid tenantId, Guid branchId, string tableNumber, double positionX = 0, double positionY = 0, double width = 100, double height = 100)
        : base(tenantId, branchId)
    {
        if (string.IsNullOrWhiteSpace(tableNumber))
            throw new ArgumentException("Table number is required.", nameof(tableNumber));

        TableNumber = tableNumber;
        PositionX = positionX;
        PositionY = positionY;
        Width = width;
        Height = height;
    }

    public void UpdateTableNumber(string tableNumber)
    {
        if (string.IsNullOrWhiteSpace(tableNumber))
            throw new ArgumentException("Table number is required.", nameof(tableNumber));

        TableNumber = tableNumber;
    }

    public void UpdatePosition(double positionX, double positionY)
    {
        PositionX = positionX;
        PositionY = positionY;
    }

    public void UpdateSize(double width, double height)
    {
        if (width <= 0)
            throw new ArgumentException("Width must be greater than zero.", nameof(width));
        if (height <= 0)
            throw new ArgumentException("Height must be greater than zero.", nameof(height));

        Width = width;
        Height = height;
    }

    public void SetAvailable()
    {
        Status = TableStatus.Available;
    }

    public void SetOccupied()
    {
        Status = TableStatus.Occupied;
    }

    public void SetReserved()
    {
        Status = TableStatus.Reserved;
    }

    public void UpdateStatus(TableStatus status)
    {
        Status = status;
    }
}
