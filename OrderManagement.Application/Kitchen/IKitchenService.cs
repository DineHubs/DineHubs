using OrderManagement.Domain.Entities;

namespace OrderManagement.Application.Kitchen;

public interface IKitchenService
{
    Task<IReadOnlyCollection<Order>> GetQueueAsync(CancellationToken cancellationToken);
}

