using OrderManagement.Application.Ordering.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Ordering;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderDto dto, Guid tenantId, Guid branchId, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<Order>> GetOrdersAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken);
    Task<Order?> GetOrderByIdAsync(Guid orderId, Guid tenantId, Guid? branchId, CancellationToken cancellationToken);
    Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid tenantId, Guid? branchId, CancellationToken cancellationToken);
    Task CancelOrderAsync(Guid orderId, string reason, Guid tenantId, Guid? branchId, CancellationToken cancellationToken);
    Task RemoveOrderLineAsync(Guid orderId, Guid lineId, Guid tenantId, Guid? branchId, CancellationToken cancellationToken);
    Task UpdateOrderLineQuantityAsync(Guid orderId, Guid lineId, int quantity, Guid tenantId, Guid? branchId, CancellationToken cancellationToken);
}

