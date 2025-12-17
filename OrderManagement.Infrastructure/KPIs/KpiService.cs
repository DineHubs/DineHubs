using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.KPIs;
using OrderManagement.Application.Ordering;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.KPIs;

public sealed class KpiService(
    OrderManagementDbContext dbContext,
    IOrderService orderService,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : IKpiService
{
    public async Task<TimeSpan?> CalculatePrepTimeAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(
                orderId,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (order is null || order.Status < OrderStatus.Ready)
            {
                return null;
            }

            // Calculate prep time from order timestamps
            // Prep time = Ready timestamp - Submitted timestamp (or InPreparation if available)
            var orderEntity = await dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.TenantId == tenantContext.TenantId, cancellationToken);

            if (orderEntity is null)
            {
                return null;
            }

            // Use CreatedAt as Submitted timestamp (since orders are created with Submitted status)
            var submittedAt = orderEntity.CreatedAt;
            
            // Find when status changed to Ready by checking UpdatedAt when status is Ready
            // For simplicity, we'll use CreatedAt to UpdatedAt when status is Ready
            // In production, you'd track status change timestamps separately
            if (orderEntity.Status == OrderStatus.Ready || orderEntity.Status >= OrderStatus.Ready)
            {
                var readyAt = orderEntity.UpdatedAt ?? DateTimeOffset.UtcNow;
                return readyAt - submittedAt;
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error calculating prep time for order {OrderId}", orderId);
            return null;
        }
    }

    public async Task<TimeSpan?> CalculateTableTurnTimeAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            var order = await orderService.GetOrderByIdAsync(
                orderId,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (order is null || order.IsTakeAway || order.Status != OrderStatus.Paid)
            {
                return null;
            }

            var orderEntity = await dbContext.Orders
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId && o.TenantId == tenantContext.TenantId, cancellationToken);

            if (orderEntity is null)
            {
                return null;
            }

            var createdAt = orderEntity.CreatedAt;
            var paidAt = orderEntity.UpdatedAt ?? DateTimeOffset.UtcNow;

            return paidAt - createdAt;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error calculating table turn time for order {OrderId}", orderId);
            return null;
        }
    }

    public async Task<bool?> GetOrderAccuracyAsync(Guid orderId, CancellationToken cancellationToken)
    {
        try
        {
            // Order accuracy is determined by comparing order lines at creation vs delivery
            // For now, we'll assume orders are accurate unless there are modifications
            // In production, this would track actual vs delivered items
            var order = await orderService.GetOrderByIdAsync(
                orderId,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (order is null)
            {
                return null;
            }

            // Check if order was modified (lines removed/updated) - indicates potential inaccuracy
            // For now, return true as default (no modification tracking yet)
            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting order accuracy for order {OrderId}", orderId);
            return null;
        }
    }

    public async Task<decimal> GetAveragePrepTimeAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await dbContext.Orders
                .AsNoTracking()
                .Where(o => o.TenantId == tenantContext.TenantId
                    && o.Status >= OrderStatus.Ready
                    && o.CreatedAt >= from
                    && o.CreatedAt <= to)
                .ToListAsync(cancellationToken);

            if (!orders.Any())
            {
                return 0;
            }

            var prepTimes = new List<TimeSpan>();
            foreach (var order in orders)
            {
                var prepTime = order.UpdatedAt - order.CreatedAt;
                if (prepTime.HasValue && prepTime.Value.TotalMinutes > 0)
                {
                    prepTimes.Add(prepTime.Value);
                }
            }

            if (!prepTimes.Any())
            {
                return 0;
            }

            var averageMinutes = prepTimes.Average(t => t.TotalMinutes);
            return (decimal)averageMinutes;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error calculating average prep time");
            return 0;
        }
    }

    public async Task<decimal> GetOrderAccuracyRateAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            var totalOrders = await dbContext.Orders
                .AsNoTracking()
                .CountAsync(o => o.TenantId == tenantContext.TenantId
                    && o.Status == OrderStatus.Paid
                    && o.CreatedAt >= from
                    && o.CreatedAt <= to, cancellationToken);

            if (totalOrders == 0)
            {
                return 100; // No orders, assume 100% accuracy
            }

            // For now, assume all orders are accurate (no modification tracking)
            // In production, count orders with modifications vs total
            var accurateOrders = totalOrders;

            return (decimal)accurateOrders / totalOrders * 100;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error calculating order accuracy rate");
            return 0;
        }
    }

    public async Task<decimal> GetAverageTableTurnTimeAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            var orders = await dbContext.Orders
                .AsNoTracking()
                .Where(o => o.TenantId == tenantContext.TenantId
                    && !o.IsTakeAway
                    && o.Status == OrderStatus.Paid
                    && o.CreatedAt >= from
                    && o.CreatedAt <= to)
                .ToListAsync(cancellationToken);

            if (!orders.Any())
            {
                return 0;
            }

            var turnTimes = new List<TimeSpan>();
            foreach (var order in orders)
            {
                var turnTime = order.UpdatedAt - order.CreatedAt;
                if (turnTime.HasValue && turnTime.Value.TotalMinutes > 0)
                {
                    turnTimes.Add(turnTime.Value);
                }
            }

            if (!turnTimes.Any())
            {
                return 0;
            }

            var averageMinutes = turnTimes.Average(t => t.TotalMinutes);
            return (decimal)averageMinutes;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error calculating average table turn time");
            return 0;
        }
    }

    public async Task<int> GetRefundFrequencyAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            var refundCount = await dbContext.Payments
                .AsNoTracking()
                .CountAsync(p => p.TenantId == tenantContext.TenantId
                    && p.Status == PaymentStatus.Refunded
                    && p.CreatedAt >= from
                    && p.CreatedAt <= to, cancellationToken);

            return refundCount;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting refund frequency");
            return 0;
        }
    }

    public async Task<int> GetReprintCountAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            var reprintCount = await dbContext.ReceiptPrints
                .AsNoTracking()
                .CountAsync(r => r.TenantId == tenantContext.TenantId
                    && r.IsReprint
                    && r.PrintedAt >= from
                    && r.PrintedAt <= to, cancellationToken);

            return reprintCount;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting reprint count");
            return 0;
        }
    }
}

