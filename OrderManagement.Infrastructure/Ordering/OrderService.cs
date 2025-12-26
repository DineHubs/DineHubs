using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Kitchen;
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
    IKitchenPrintService kitchenPrintService,
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

            // Check if table is already occupied (has active order) for dine-in orders
            if (!dto.IsTakeAway)
            {
                var activeStatuses = new[] 
                { 
                    OrderStatus.Draft, 
                    OrderStatus.Submitted, 
                    OrderStatus.InPreparation, 
                    OrderStatus.Ready, 
                    OrderStatus.Delivered 
                };
                
                var existingOrder = await dbContext.Orders
                    .AsNoTracking()
                    .Where(o => o.TenantId == tenantId 
                        && o.BranchId == branchId 
                        && o.TableNumber == tableNumber 
                        && activeStatuses.Contains(o.Status))
                    .FirstOrDefaultAsync(cancellationToken);
                
                if (existingOrder != null)
                {
                    logger.Warning("Table {TableNumber} already has active order {OrderNumber} for tenant {TenantId}, branch {BranchId}", 
                        tableNumber, existingOrder.OrderNumber, tenantId, branchId);
                    throw new InvalidOperationException(
                        $"Table {tableNumber} already has an active order ({existingOrder.OrderNumber}). Please complete or cancel that order first.");
                }
            }

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

            // Auto-print kitchen ticket for submitted orders
            try
            {
                var printResult = await kitchenPrintService.PrintKitchenTicketAsync(order.Id, cancellationToken);
                if (printResult.Success)
                {
                    logger.Information("Auto-printed kitchen ticket for order {OrderNumber}. PrintJobId: {PrintJobId}", 
                        orderNumber, printResult.PrintJobId);
                }
                else
                {
                    logger.Warning("Failed to auto-print kitchen ticket for order {OrderNumber}: {Message}", 
                        orderNumber, printResult.Message);
                }
            }
            catch (Exception printEx)
            {
                // Don't fail order creation if printing fails
                logger.Warning(printEx, "Error auto-printing kitchen ticket for order {OrderNumber}", orderNumber);
            }

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

            // Enforce payment timing validation when transitioning to InPreparation
            if (status == OrderStatus.InPreparation && order.PaymentTiming == PaymentTiming.PayBeforeKitchen)
            {
                // Query payment directly to avoid circular dependency with PaymentService
                var payment = await dbContext.Payments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.OrderId == orderId && p.TenantId == tenantId, cancellationToken);
                    
                if (payment is null || (payment.Status != PaymentStatus.Captured && payment.Status != PaymentStatus.Authorized))
                {
                    logger.Warning("Order {OrderId} requires payment before kitchen submission but payment is not complete", orderId);
                    throw new InvalidOperationException("Payment is required before sending this order to the kitchen. Please process payment first.");
                }
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

    public async Task<CanSubmitToKitchenResult> CanSubmitToKitchenAsync(Guid orderId, Guid tenantId, Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            // Query order with tenant/branch filtering
            var query = dbContext.Orders.AsNoTracking().Where(o => o.Id == orderId && o.TenantId == tenantId);
            
            if (branchId.HasValue)
            {
                query = query.Where(o => o.BranchId == branchId.Value);
            }
            
            var order = await query.FirstOrDefaultAsync(cancellationToken);
            if (order is null)
            {
                return new CanSubmitToKitchenResult(false, false, null, "Order not found.");
            }

            // If order doesn't require payment before kitchen, it can be submitted
            if (order.PaymentTiming != PaymentTiming.PayBeforeKitchen)
            {
                return new CanSubmitToKitchenResult(true, false, null, null);
            }

            // Check payment status - query directly to avoid circular dependency
            var payment = await dbContext.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.TenantId == tenantId, cancellationToken);
                
            if (payment is null)
            {
                return new CanSubmitToKitchenResult(false, true, "None", "Payment is required before sending this order to the kitchen.");
            }

            var paymentStatusText = payment.Status.ToString();
            if (payment.Status == PaymentStatus.Captured || payment.Status == PaymentStatus.Authorized)
            {
                return new CanSubmitToKitchenResult(true, true, paymentStatusText, null);
            }

            return new CanSubmitToKitchenResult(false, true, paymentStatusText, $"Payment status is {paymentStatusText}. Payment must be Authorized or Captured before sending to kitchen.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error checking if order {OrderId} can be submitted to kitchen for tenant {TenantId}, branch {BranchId}", 
                orderId, tenantId, branchId);
            return new CanSubmitToKitchenResult(false, false, null, "An error occurred while checking kitchen submission eligibility.");
        }
    }
}

