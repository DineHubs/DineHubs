using OrderManagement.Domain.Common;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Domain.Entities;

public class PaymentTransaction : BranchScopedEntity
{
    public Guid OrderId { get; private set; }
    public string Provider { get; private set; } = string.Empty;
    public string Reference { get; private set; } = string.Empty;
    public PaymentStatus Status { get; private set; } = PaymentStatus.Pending;
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } = "MYR";
    public string? ReceiptUrl { get; private set; }

    private PaymentTransaction()
    {
    }

    public PaymentTransaction(Guid tenantId, Guid branchId, Guid orderId, string provider, decimal amount, string currency)
        : base(tenantId, branchId)
    {
        OrderId = orderId;
        Provider = provider;
        Amount = amount;
        Currency = currency;
    }

    public void MarkAuthorized(string reference)
    {
        Status = PaymentStatus.Authorized;
        Reference = reference;
    }

    public void MarkCaptured(string receiptUrl)
    {
        Status = PaymentStatus.Captured;
        ReceiptUrl = receiptUrl;
    }

    public void MarkFailed() => Status = PaymentStatus.Failed;

    public void Refund(string? reason = null)
    {
        if (Status != PaymentStatus.Captured)
        {
            throw new InvalidOperationException("Only captured payments can be refunded.");
        }
        Status = PaymentStatus.Refunded;
    }

    public void Void(string? reason = null)
    {
        if (Status != PaymentStatus.Authorized && Status != PaymentStatus.Pending)
        {
            throw new InvalidOperationException("Only authorized or pending payments can be voided.");
        }
        Status = PaymentStatus.Voided;
    }
}


