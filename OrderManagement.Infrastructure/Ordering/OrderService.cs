using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Ordering;
using OrderManagement.Application.Ordering.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Ordering;

public sealed class OrderService(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : IOrderService
{
    public async Task<Order> CreateOrderAsync(CreateOrderDto dto, Guid tenantId, Guid branchId, CancellationToken cancellationToken)
    {
        try
        {
            // Validate TableNumber: required for dine-in orders, optional for takeaway
            if (!dto.IsTakeAway && string.IsNullOrWhiteSpace(dto.TableNumber))
            {
                throw new InvalidOperationException("Table number is required for dine-in orders.");
            }

            // Use empty string for takeaway orders when TableNumber is null
            var tableNumber = dto.IsTakeAway 
                ? (dto.TableNumber ?? string.Empty)
                : dto.TableNumber!;

            // Generate order number
            var orderNumber = $"OM-{DateTimeOffset.UtcNow:yyyyMMddHHmmss}";

            // Create order
            var order = new Order(
                tenantId,
                branchId,
                orderNumber,
                dto.IsTakeAway,
                tableNumber);

            // Add order lines
            foreach (var item in dto.Items)
            {
                order.AddLine(item.MenuItemId, item.Name, item.Price, item.Quantity);
            }

            // Set order status to Submitted so it appears in kitchen queue
            order.UpdateStatus(OrderStatus.Submitted);

            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.Information("Created order {OrderNumber} (Id: {OrderId}) for tenant {TenantId}, branch {BranchId}", 
                orderNumber, order.Id, tenantId, branchId);

            return order;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating order for tenant {TenantId}, branch {BranchId}", tenantId, branchId);
            throw new InvalidOperationException("An error occurred while creating the order.");
        }
    }

    public async Task<IReadOnlyCollection<Order>> GetOrdersAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.TenantId == tenantId);

            if (branchId.HasValue)
            {
                query = query.Where(o => o.BranchId == branchId.Value);
            }

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(cancellationToken);

            logger.Information("Retrieved {Count} orders for tenant {TenantId}, branch {BranchId}", 
                orders.Count, tenantId, branchId);

            return orders;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving orders for tenant {TenantId}, branch {BranchId}", tenantId, branchId);
            throw new InvalidOperationException("An error occurred while retrieving orders.");
        }
    }

    public async Task<Order?> GetOrderByIdAsync(Guid orderId, Guid tenantId, Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.Id == orderId && o.TenantId == tenantId);

            if (branchId.HasValue)
            {
                query = query.Where(o => o.BranchId == branchId.Value);
            }

            var order = await query.FirstOrDefaultAsync(cancellationToken);

            if (order is null)
            {
                logger.Warning("Order {OrderId} not found for tenant {TenantId}, branch {BranchId}", 
                    orderId, tenantId, branchId);
            }

            return order;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving order {OrderId} for tenant {TenantId}, branch {BranchId}", 
                orderId, tenantId, branchId);
            throw new InvalidOperationException("An error occurred while retrieving the order.");
        }
    }

    public async Task UpdateOrderStatusAsync(Guid orderId, OrderStatus status, Guid tenantId, Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            // Query order with tenant/branch filtering to ensure security
            var query = dbContext.Orders.Where(o => o.Id == orderId && o.TenantId == tenantId);
            
            if (branchId.HasValue)
            {
                query = query.Where(o => o.BranchId == branchId.Value);
            }
            
            var order = await query.FirstOrDefaultAsync(cancellationToken);
            if (order is null)
            {
                logger.Warning("Order {OrderId} not found for tenant {TenantId}, branch {BranchId} when updating status", 
                    orderId, tenantId, branchId);
                throw new InvalidOperationException("Order not found.");
            }

            order.UpdateStatus(status);
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.Information("Updated order {OrderId} status to {Status} for tenant {TenantId}, branch {BranchId}", 
                orderId, status, tenantId, branchId);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating order {OrderId} status to {Status} for tenant {TenantId}, branch {BranchId}", 
                orderId, status, tenantId, branchId);
            throw new InvalidOperationException("An error occurred while updating the order status.");
        }
    }
}

