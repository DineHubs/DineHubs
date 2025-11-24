using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class QrOrderSession : BranchScopedEntity
{
    public string SessionCode { get; private set; } = string.Empty;
    public string TableNumber { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTimeOffset ExpiresAt { get; private set; }

    private QrOrderSession()
    {
    }

    public QrOrderSession(Guid tenantId, Guid branchId, string sessionCode, string tableNumber, DateTimeOffset expiresAt)
        : base(tenantId, branchId)
    {
        SessionCode = sessionCode;
        TableNumber = tableNumber;
        ExpiresAt = expiresAt;
    }

    public void Close() => IsActive = false;
}


