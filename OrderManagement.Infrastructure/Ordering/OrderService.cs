using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.MenuItems;
using OrderManagement.Application.Ordering;
using OrderManagement.Application.Ordering.Models;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Ordering;

public sealed class OrderService(
    OrderManagementDbContext dbContext,
    IMenuItemService menuItemService,
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

            // Validate menu item availability before creating order
            var unavailableItems = new List<string>();
            foreach (var item in dto.Items)
            {
                var menuItem = await menuItemService.GetMenuItemByIdAsync(item.MenuItemId, cancellationToken);
                if (menuItem is null)
                {
                    logger.Warning("Menu item {MenuItemId} not found when creating order", item.MenuItemId);
                    throw new InvalidOperationException($"Menu item '{item.Name}' not found. Please check the menu and try again.");
                }
                
                if (!menuItem.IsAvailable)
                {
                    unavailableItems.Add(item.Name);
                    logger.Warning("Attempted to add unavailable menu item {MenuItemId} ({Name}) to order", item.MenuItemId, item.Name);
                }
            }

            if (unavailableItems.Any())
            {
                var itemsList = string.Join(", ", unavailableItems);
                throw new InvalidOperationException($"The following items are currently unavailable: {itemsList}. Please remove them from your order or check the menu for alternatives.");
            }

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

    public async Task CancelOrderAsync(Guid orderId, string reason, Guid tenantId, Guid? branchId, CancellationToken cancellationToken)
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
                logger.Warning("Order {OrderId} not found for tenant {TenantId}, branch {BranchId} when cancelling", 
                    orderId, tenantId, branchId);
                throw new InvalidOperationException("Order not found.");
            }

            // Use domain method which validates cancellation rules
            order.Cancel(reason);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.Information("Cancelled order {OrderId} with reason: {Reason} for tenant {TenantId}, branch {BranchId}", 
                orderId, reason, tenantId, branchId);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error cancelling order {OrderId} for tenant {TenantId}, branch {BranchId}", 
                orderId, tenantId, branchId);
            throw new InvalidOperationException("An error occurred while cancelling the order.");
        }
    }

    public async Task RemoveOrderLineAsync(Guid orderId, Guid lineId, Guid tenantId, Guid? branchId, CancellationToken cancellationToken)
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
                logger.Warning("Order {OrderId} not found for tenant {TenantId}, branch {BranchId} when removing line", 
                    orderId, tenantId, branchId);
                throw new InvalidOperationException("Order not found.");
            }

            // Use domain method which validates modification rules
            order.RemoveLine(lineId);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.Information("Removed line {LineId} from order {OrderId} for tenant {TenantId}, branch {BranchId}", 
                lineId, orderId, tenantId, branchId);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error removing line {LineId} from order {OrderId} for tenant {TenantId}, branch {BranchId}", 
                lineId, orderId, tenantId, branchId);
            throw new InvalidOperationException("An error occurred while removing the order line.");
        }
    }

    public async Task UpdateOrderLineQuantityAsync(Guid orderId, Guid lineId, int quantity, Guid tenantId, Guid? branchId, CancellationToken cancellationToken)
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
                logger.Warning("Order {OrderId} not found for tenant {TenantId}, branch {BranchId} when updating line quantity", 
                    orderId, tenantId, branchId);
                throw new InvalidOperationException("Order not found.");
            }

            // Use domain method which validates modification rules
            order.UpdateLineQuantity(lineId, quantity);
            
            await dbContext.SaveChangesAsync(cancellationToken);
            
            logger.Information("Updated line {LineId} quantity to {Quantity} in order {OrderId} for tenant {TenantId}, branch {BranchId}", 
                lineId, quantity, orderId, tenantId, branchId);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating line {LineId} quantity in order {OrderId} for tenant {TenantId}, branch {BranchId}", 
                lineId, orderId, tenantId, branchId);
            throw new InvalidOperationException("An error occurred while updating the order line quantity.");
        }
    }
}

