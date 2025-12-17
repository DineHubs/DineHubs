namespace OrderManagement.Application.KPIs;

public interface IKpiService
{
    Task<TimeSpan?> CalculatePrepTimeAsync(Guid orderId, CancellationToken cancellationToken);
    Task<TimeSpan?> CalculateTableTurnTimeAsync(Guid orderId, CancellationToken cancellationToken);
    Task<bool?> GetOrderAccuracyAsync(Guid orderId, CancellationToken cancellationToken);
    Task<decimal> GetAveragePrepTimeAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<decimal> GetOrderAccuracyRateAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<decimal> GetAverageTableTurnTimeAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<int> GetRefundFrequencyAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
    Task<int> GetReprintCountAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken);
}

