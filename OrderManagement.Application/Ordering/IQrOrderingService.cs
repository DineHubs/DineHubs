namespace OrderManagement.Application.Ordering;

public interface IQrOrderingService
{
    Task<string> GenerateSessionAsync(Guid tenantId, Guid branchId, string tableNumber, CancellationToken cancellationToken);
    Task CloseSessionAsync(string sessionCode, CancellationToken cancellationToken);
}


