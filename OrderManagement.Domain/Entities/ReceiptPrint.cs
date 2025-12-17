using OrderManagement.Domain.Common;

namespace OrderManagement.Domain.Entities;

public class ReceiptPrint : BranchScopedEntity
{
    public Guid OrderId { get; private set; }
    public Guid PaymentId { get; private set; }
    public DateTimeOffset PrintedAt { get; private set; }
    public Guid? PrintedBy { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public bool IsReprint { get; private set; }
    public string ReceiptUrl { get; private set; } = string.Empty;

    private ReceiptPrint()
    {
    }

    public ReceiptPrint(
        Guid tenantId,
        Guid branchId,
        Guid orderId,
        Guid paymentId,
        string receiptUrl,
        string reason,
        bool isReprint,
        Guid? printedBy = null)
        : base(tenantId, branchId)
    {
        OrderId = orderId;
        PaymentId = paymentId;
        ReceiptUrl = receiptUrl;
        Reason = reason;
        IsReprint = isReprint;
        PrintedBy = printedBy;
        PrintedAt = DateTimeOffset.UtcNow;
    }
}

