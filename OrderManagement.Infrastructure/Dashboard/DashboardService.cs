using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Dashboard;
using OrderManagement.Domain.Enums;
using OrderManagement.Identity.Entities;
using OrderManagement.Infrastructure.Persistence;
using Serilog;

namespace OrderManagement.Infrastructure.Dashboard;

public sealed class DashboardService(
    OrderManagementDbContext dbContext,
    ITenantContext tenantContext,
    ICurrentUserContext currentUserContext,
    Serilog.ILogger logger) : IDashboardService
{
    public async Task<DashboardStatsDto> GetDashboardStatsAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            var today = DateTimeOffset.UtcNow.Date;
            var todayStart = new DateTimeOffset(today);
            var todayEnd = todayStart.AddDays(1).AddTicks(-1);
            
            var thisMonth = new DateTimeOffset(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, 1, 0, 0, 0, TimeSpan.Zero);
            var thisMonthEnd = thisMonth.AddMonths(1).AddTicks(-1);

            // Base query for orders filtered by tenant
            var ordersQuery = dbContext.Orders.AsNoTracking().Where(o => o.TenantId == tenantContext.TenantId);
            if (tenantContext.BranchId.HasValue)
            {
                ordersQuery = ordersQuery.Where(o => o.BranchId == tenantContext.BranchId.Value);
            }

            // Today's orders count
            var todayOrdersCount = 0;
            try
            {
                todayOrdersCount = await ordersQuery
                    .CountAsync(o => o.CreatedAt >= todayStart && o.CreatedAt <= todayEnd, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving today's orders count for tenant {TenantId}, branch {BranchId}, defaulting to 0", 
                    tenantContext.TenantId, tenantContext.BranchId);
                todayOrdersCount = 0;
            }

            // Today's revenue
            var todayRevenue = 0m;
            try
            {
                todayRevenue = await ordersQuery
                    .Where(o => o.CreatedAt >= todayStart && o.CreatedAt <= todayEnd)
                    .SumAsync(o => (decimal?)(o.Subtotal + o.Tax + o.ServiceCharge), cancellationToken) ?? 0m;
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving today's revenue for tenant {TenantId}, branch {BranchId}, defaulting to 0", 
                    tenantContext.TenantId, tenantContext.BranchId);
                todayRevenue = 0m;
            }

            // Pending orders (Submitted or InPreparation)
            var pendingOrdersCount = 0;
            try
            {
                pendingOrdersCount = await ordersQuery
                    .CountAsync(o => o.Status == OrderStatus.Submitted || o.Status == OrderStatus.InPreparation, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving pending orders count for tenant {TenantId}, branch {BranchId}, defaulting to 0", 
                    tenantContext.TenantId, tenantContext.BranchId);
                pendingOrdersCount = 0;
            }

            // Orders in queue (same as pending)
            var ordersInQueueCount = pendingOrdersCount;

            // Active tables (orders with non-empty table numbers that are not completed/cancelled)
            var activeTablesCount = 0;
            try
            {
                activeTablesCount = await ordersQuery
                    .Where(o => !string.IsNullOrEmpty(o.TableNumber) && 
                               o.Status != OrderStatus.Delivered && 
                               o.Status != OrderStatus.Paid && 
                               o.Status != OrderStatus.Cancelled)
                    .Select(o => o.TableNumber)
                    .Distinct()
                    .CountAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving active tables count for tenant {TenantId}, branch {BranchId}, defaulting to 0", 
                    tenantContext.TenantId, tenantContext.BranchId);
                activeTablesCount = 0;
            }

            // This month's revenue
            var thisMonthRevenue = 0m;
            try
            {
                thisMonthRevenue = await ordersQuery
                    .Where(o => o.CreatedAt >= thisMonth && o.CreatedAt <= thisMonthEnd)
                    .SumAsync(o => (decimal?)(o.Subtotal + o.Tax + o.ServiceCharge), cancellationToken) ?? 0m;
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving this month's revenue for tenant {TenantId}, branch {BranchId}, defaulting to 0", 
                    tenantContext.TenantId, tenantContext.BranchId);
                thisMonthRevenue = 0m;
            }

            // This month's orders count
            var thisMonthOrdersCount = 0;
            try
            {
                thisMonthOrdersCount = await ordersQuery
                    .CountAsync(o => o.CreatedAt >= thisMonth && o.CreatedAt <= thisMonthEnd, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving this month's orders count for tenant {TenantId}, branch {BranchId}, defaulting to 0", 
                    tenantContext.TenantId, tenantContext.BranchId);
                thisMonthOrdersCount = 0;
            }

            // Active branches count
            var activeBranchesCount = 0;
            try
            {
                var branchesQuery = dbContext.Branches.AsNoTracking().Where(b => b.TenantId == tenantContext.TenantId && b.IsActive);
                activeBranchesCount = await branchesQuery.CountAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving active branches count for tenant {TenantId}, defaulting to 0", tenantContext.TenantId);
                activeBranchesCount = 0;
            }

            // Active users count (not locked out)
            var activeUsersCount = 0;
            try
            {
                activeUsersCount = await dbContext.Users
                    .AsNoTracking()
                    .CountAsync(u => u.TenantId == tenantContext.TenantId && u.LockoutEnd == null, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving active users count for tenant {TenantId}, defaulting to 0", tenantContext.TenantId);
                activeUsersCount = 0;
            }

            // Inventory value
            var inventoryValue = 0m;
            try
            {
                var inventoryItems = await dbContext.InventoryItems
                    .AsNoTracking()
                    .Where(i => i.TenantId == tenantContext.TenantId)
                    .ToListAsync(cancellationToken);
                
                // Note: Inventory value calculation would need price per unit - placeholder for now
                // TODO: Calculate if price information is available
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving inventory items for tenant {TenantId}, defaulting inventory value to 0", tenantContext.TenantId);
                inventoryValue = 0m;
            }

            // Low stock items count
            var lowStockItemsCount = 0;
            try
            {
                lowStockItemsCount = await dbContext.InventoryItems
                    .AsNoTracking()
                    .CountAsync(i => i.TenantId == tenantContext.TenantId && i.QuantityOnHand <= i.ReorderPoint, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Error retrieving low stock items count for tenant {TenantId}, defaulting to 0", tenantContext.TenantId);
                lowStockItemsCount = 0;
            }

            // Waiter-specific stats (My orders today)
            var myOrdersTodayCount = 0;
            var myOrdersTodayRevenue = 0m;
            if (currentUserContext.UserId.HasValue)
            {
                try
                {
                    var myOrdersQuery = ordersQuery.Where(o => o.CreatedBy == currentUserContext.UserId.Value);
                    myOrdersTodayCount = await myOrdersQuery
                        .CountAsync(o => o.CreatedAt >= todayStart && o.CreatedAt <= todayEnd, cancellationToken);
                    
                    myOrdersTodayRevenue = await myOrdersQuery
                        .Where(o => o.CreatedAt >= todayStart && o.CreatedAt <= todayEnd)
                        .SumAsync(o => (decimal?)(o.Subtotal + o.Tax + o.ServiceCharge), cancellationToken) ?? 0m;
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, "Error retrieving waiter-specific stats for user {UserId}, defaulting to 0", currentUserContext.UserId.Value);
                }
            }

            // SuperAdmin-specific stats (subscription metrics)
            var activeTenantsCount = 0;
            var newSubscriptionsThisMonth = 0;
            var totalActiveSubscriptions = 0;
            
            // Check if user is SuperAdmin (tenantId is Guid.Empty for SuperAdmin)
            if (tenantContext.TenantId == Guid.Empty)
            {
                try
                {
                    // Total active subscriptions
                    totalActiveSubscriptions = await dbContext.Subscriptions
                        .AsNoTracking()
                        .CountAsync(s => s.Status == SubscriptionStatus.Active, cancellationToken);
                    
                    // Active tenants (tenants with active subscriptions)
                    activeTenantsCount = await dbContext.Subscriptions
                        .AsNoTracking()
                        .Where(s => s.Status == SubscriptionStatus.Active)
                        .Select(s => s.TenantId)
                        .Distinct()
                        .CountAsync(cancellationToken);
                    
                    // New subscriptions this month
                    newSubscriptionsThisMonth = await dbContext.Subscriptions
                        .AsNoTracking()
                        .CountAsync(s => s.CreatedAt >= thisMonth && s.CreatedAt <= thisMonthEnd, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.Warning(ex, "Error retrieving SuperAdmin subscription stats, defaulting to 0");
                }
            }

            logger.Information("Retrieved dashboard stats for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);

            return new DashboardStatsDto
            {
                TodayOrdersCount = todayOrdersCount,
                TodayRevenue = todayRevenue,
                PendingOrdersCount = pendingOrdersCount,
                ActiveTablesCount = activeTablesCount,
                ThisMonthRevenue = thisMonthRevenue,
                ThisMonthOrdersCount = thisMonthOrdersCount,
                ActiveBranchesCount = activeBranchesCount,
                ActiveUsersCount = activeUsersCount,
                OrdersInQueueCount = ordersInQueueCount,
                InventoryValue = inventoryValue,
                LowStockItemsCount = lowStockItemsCount,
                MyOrdersTodayCount = myOrdersTodayCount,
                MyOrdersTodayRevenue = myOrdersTodayRevenue,
                ActiveTenantsCount = activeTenantsCount,
                NewSubscriptionsThisMonth = newSubscriptionsThisMonth,
                TotalActiveSubscriptions = totalActiveSubscriptions
            };
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving dashboard stats for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<SalesTrendDto>> GetSalesTrendAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            // Convert to UTC to avoid PostgreSQL offset issues
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();

            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.TenantId == tenantContext.TenantId && 
                           o.CreatedAt >= fromUtc && 
                           o.CreatedAt <= toUtc);

            if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == tenantContext.BranchId.Value);
            }

            var orders = await query
                .ToListAsync(cancellationToken);

            // Group by date in memory after converting to UTC
            var results = orders
                .GroupBy(o => o.CreatedAt.ToUniversalTime().Date)
                .Select(g => new SalesTrendDto
                {
                    Date = new DateTimeOffset(g.Key, TimeSpan.Zero),
                    Revenue = g.Sum(o => o.Subtotal + o.Tax + o.ServiceCharge),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            logger.Information("Retrieved sales trend for tenant {TenantId}, branch {BranchId} from {From} to {To}", 
                tenantContext.TenantId, tenantContext.BranchId, from, to);

            return results;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving sales trend for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<TopSellingItemDto>> GetTopSellingItemsAsync(int count, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            // Convert to UTC to avoid PostgreSQL offset issues
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();

            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.TenantId == tenantContext.TenantId && 
                           o.CreatedAt >= fromUtc && 
                           o.CreatedAt <= toUtc);

            if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == tenantContext.BranchId.Value);
            }

            // Use SelectMany to flatten order lines
            var results = await query
                .SelectMany(o => o.Lines.Select(l => new
                {
                    l.MenuItemId,
                    l.Name,
                    l.Quantity,
                    l.Price
                }))
                .GroupBy(l => new { l.MenuItemId, l.Name })
                .Select(g => new TopSellingItemDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    Name = g.Key.Name,
                    TotalQuantity = g.Sum(l => l.Quantity),
                    TotalRevenue = g.Sum(l => l.Price * l.Quantity)
                })
                .OrderByDescending(i => i.TotalQuantity)
                .Take(count)
                .ToListAsync(cancellationToken);

            logger.Information("Retrieved top {Count} selling items for tenant {TenantId}, branch {BranchId}", 
                count, tenantContext.TenantId, tenantContext.BranchId);

            return results;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving top selling items for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<OrderStatusCountDto>> GetOrdersByStatusAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            // Convert to UTC to avoid PostgreSQL offset issues
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();

            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.TenantId == tenantContext.TenantId && 
                           o.CreatedAt >= fromUtc && 
                           o.CreatedAt <= toUtc);

            if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == tenantContext.BranchId.Value);
            }

            var results = await query
                .GroupBy(o => o.Status)
                .Select(g => new OrderStatusCountDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            logger.Information("Retrieved order status counts for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);

            return results;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving order status counts for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<OrderHourlyCountDto>> GetOrdersByHourAsync(DateTimeOffset date, CancellationToken cancellationToken)
    {
        try
        {
            // Convert to UTC to avoid PostgreSQL offset issues
            var dateUtc = date.ToUniversalTime();
            var dayStart = new DateTimeOffset(dateUtc.Date, TimeSpan.Zero);
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);

            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.TenantId == tenantContext.TenantId && 
                           o.CreatedAt >= dayStart && 
                           o.CreatedAt <= dayEnd);

            if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == tenantContext.BranchId.Value);
            }

            var results = await query
                .ToListAsync(cancellationToken);

            // Group by hour in memory after converting to UTC
            var hourlyGroups = results
                .GroupBy(o => o.CreatedAt.ToUniversalTime().Hour)
                .Select(g => new OrderHourlyCountDto
                {
                    Hour = g.Key,
                    OrderCount = g.Count()
                })
                .OrderBy(h => h.Hour)
                .ToList();

            // Fill in missing hours with 0
            var allHours = Enumerable.Range(0, 24)
                .Select(h => new OrderHourlyCountDto
                {
                    Hour = h,
                    OrderCount = hourlyGroups.FirstOrDefault(r => r.Hour == h)?.OrderCount ?? 0
                })
                .ToList();

            logger.Information("Retrieved hourly order counts for tenant {TenantId}, branch {BranchId} on {Date}", 
                tenantContext.TenantId, tenantContext.BranchId, date);

            return allHours;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving hourly order counts for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<LowStockItemDto>> GetLowStockItemsAsync(CancellationToken cancellationToken)
    {
        try
        {
            var items = await dbContext.InventoryItems
                .AsNoTracking()
                .Where(i => i.TenantId == tenantContext.TenantId && 
                           i.QuantityOnHand <= i.ReorderPoint)
                .Select(i => new LowStockItemDto
                {
                    ItemId = i.Id,
                    Name = i.Name,
                    QuantityOnHand = i.QuantityOnHand,
                    ReorderPoint = i.ReorderPoint,
                    QuantityNeeded = i.ReorderPoint - i.QuantityOnHand
                })
                .OrderBy(i => i.QuantityNeeded)
                .ToListAsync(cancellationToken);

            logger.Information("Retrieved {Count} low stock items for tenant {TenantId}", 
                items.Count, tenantContext.TenantId);

            return items;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving low stock items for tenant {TenantId}", tenantContext.TenantId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<RevenueByDayDto>> GetRevenueByDayAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            // Convert to UTC to avoid PostgreSQL offset issues
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();

            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.TenantId == tenantContext.TenantId && 
                           o.CreatedAt >= fromUtc && 
                           o.CreatedAt <= toUtc);

            if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == tenantContext.BranchId.Value);
            }

            var orders = await query
                .ToListAsync(cancellationToken);

            // Group by date in memory after converting to UTC
            var results = orders
                .GroupBy(o => o.CreatedAt.ToUniversalTime().Date)
                .Select(g => new RevenueByDayDto
                {
                    Date = new DateTimeOffset(g.Key, TimeSpan.Zero),
                    Revenue = g.Sum(o => o.Subtotal + o.Tax + o.ServiceCharge),
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            logger.Information("Retrieved revenue by day for tenant {TenantId}, branch {BranchId} from {From} to {To}", 
                tenantContext.TenantId, tenantContext.BranchId, from, to);

            return results;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving revenue by day for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<AverageOrderValueDto>> GetAverageOrderValueTrendAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken)
    {
        try
        {
            // Convert to UTC to avoid PostgreSQL offset issues
            var fromUtc = from.ToUniversalTime();
            var toUtc = to.ToUniversalTime();

            var query = dbContext.Orders.AsNoTracking()
                .Where(o => o.TenantId == tenantContext.TenantId && 
                           o.CreatedAt >= fromUtc && 
                           o.CreatedAt <= toUtc);

            if (tenantContext.BranchId.HasValue)
            {
                query = query.Where(o => o.BranchId == tenantContext.BranchId.Value);
            }

            var orders = await query
                .ToListAsync(cancellationToken);

            // Group by date in memory after converting to UTC
            var results = orders
                .GroupBy(o => o.CreatedAt.ToUniversalTime().Date)
                .Select(g => new AverageOrderValueDto
                {
                    Date = new DateTimeOffset(g.Key, TimeSpan.Zero),
                    AverageOrderValue = g.Any() ? g.Average(o => o.Subtotal + o.Tax + o.ServiceCharge) : 0m,
                    OrderCount = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToList();

            logger.Information("Retrieved average order value trend for tenant {TenantId}, branch {BranchId} from {From} to {To}", 
                tenantContext.TenantId, tenantContext.BranchId, from, to);

            return results;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving average order value trend for tenant {TenantId}, branch {BranchId}", 
                tenantContext.TenantId, tenantContext.BranchId);
            throw;
        }
    }

    public async Task<IReadOnlyCollection<SubscriptionStatusCountDto>> GetSubscriptionStatusBreakdownAsync(CancellationToken cancellationToken)
    {
        try
        {
            var results = await dbContext.Subscriptions
                .AsNoTracking()
                .GroupBy(s => s.Status)
                .Select(g => new SubscriptionStatusCountDto
                {
                    Status = g.Key.ToString(),
                    Count = g.Count()
                })
                .ToListAsync(cancellationToken);

            logger.Information("Retrieved subscription status breakdown with {Count} statuses", results.Count);

            return results;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving subscription status breakdown");
            throw;
        }
    }

    public async Task<IReadOnlyCollection<SubscriptionTrendDto>> GetSubscriptionTrendAsync(int months, CancellationToken cancellationToken)
    {
        try
        {
            var startDate = DateTimeOffset.UtcNow.AddMonths(-months).Date;
            var startDateOffset = new DateTimeOffset(startDate, TimeSpan.Zero);

            var subscriptions = await dbContext.Subscriptions
                .AsNoTracking()
                .Where(s => s.CreatedAt >= startDateOffset)
                .ToListAsync(cancellationToken);

            // Group by month in memory
            var results = subscriptions
                .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
                .Select(g => new SubscriptionTrendDto
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Count = g.Count()
                })
                .OrderBy(t => t.Month)
                .ToList();

            // Fill in missing months with 0
            var allMonths = new List<SubscriptionTrendDto>();
            for (int i = months - 1; i >= 0; i--)
            {
                var monthDate = DateTimeOffset.UtcNow.AddMonths(-i);
                var monthKey = $"{monthDate.Year}-{monthDate.Month:D2}";
                var existing = results.FirstOrDefault(r => r.Month == monthKey);
                
                allMonths.Add(new SubscriptionTrendDto
                {
                    Month = monthKey,
                    Count = existing?.Count ?? 0
                });
            }

            logger.Information("Retrieved subscription trend for last {Months} months", months);

            return allMonths;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving subscription trend");
            throw;
        }
    }
}

