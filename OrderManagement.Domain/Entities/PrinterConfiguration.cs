using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class PrinterConfiguration : TenantScopedEntity
{
    public Guid BranchId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public PrinterType Type { get; private set; }
    public ConnectionType ConnectionType { get; private set; }
    public string PrinterName { get; private set; } = string.Empty;
    public int PaperWidth { get; private set; } = 80;
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; } = true;

    private PrinterConfiguration()
    {
    }

    public PrinterConfiguration(
        Guid tenantId,
        Guid branchId,
        string name,
        PrinterType type,
        ConnectionType connectionType,
        string printerName,
        int paperWidth = 80,
        bool isDefault = false) : base(tenantId)
    {
        BranchId = branchId;
        Name = name;
        Type = type;
        ConnectionType = connectionType;
        PrinterName = printerName;
        PaperWidth = paperWidth;
        IsDefault = isDefault;
    }

    public void Update(
        string name,
        PrinterType type,
        ConnectionType connectionType,
        string printerName,
        int paperWidth,
        bool isDefault)
    {
        Name = name;
        Type = type;
        ConnectionType = connectionType;
        PrinterName = printerName;
        PaperWidth = paperWidth;
        IsDefault = isDefault;
    }

    public void SetAsDefault() => IsDefault = true;

    public void RemoveDefault() => IsDefault = false;

    public void Activate() => IsActive = true;

    public void Deactivate() => IsActive = false;
}

