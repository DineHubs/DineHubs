namespace OrderManagement.Application.Receipts;

public interface IReceiptService
{
    Task<string> GenerateReceiptAsync(
        Guid orderId,
        CancellationToken cancellationToken);

    Task<string> ReprintReceiptAsync(
        Guid orderId,
        string reason,
        Guid? printedBy,
        CancellationToken cancellationToken);

    Task<string?> GetReceiptUrlAsync(
        Guid orderId,
        CancellationToken cancellationToken);
}

