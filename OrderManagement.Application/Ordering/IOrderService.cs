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
}

