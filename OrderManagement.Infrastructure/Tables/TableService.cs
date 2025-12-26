using Microsoft.EntityFrameworkCore;
using OrderManagement.Application.Tables;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Tables;

public class TableService(OrderManagementDbContext context) : ITableService
{
    private const int MinTablesPerBranch = 2;
    private const int MaxTablesPerBranch = 50;

    public async Task<IEnumerable<TableDto>> GetTablesAsync(
        Guid tenantId,
        Guid? userBranchId,
        Guid? requestedBranchId,
        CancellationToken cancellationToken = default)
    {
        // Determine which branch to query
        // If user has a branch assigned (Manager/Waiter), use that
        // If user is Admin (no branch assigned), use requested branch or get all
        var branchId = userBranchId ?? requestedBranchId;

        var query = context.Tables
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId);

        if (branchId.HasValue)
        {
            query = query.Where(t => t.BranchId == branchId.Value);
        }

        var tables = await query
            .OrderBy(t => t.TableNumber)
            .ToListAsync(cancellationToken);

        return tables.Select(MapToDto);
    }

    public async Task<TableDto?> GetTableByIdAsync(
        Guid tableId,
        Guid tenantId,
        Guid? userBranchId,
        CancellationToken cancellationToken = default)
    {
        var query = context.Tables
            .AsNoTracking()
            .Where(t => t.TenantId == tenantId)
            .Where(t => t.Id == tableId);

        // Apply branch filter if user is Manager/Waiter
        if (userBranchId.HasValue)
        {
            query = query.Where(t => t.BranchId == userBranchId.Value);
        }

        var table = await query.FirstOrDefaultAsync(cancellationToken);
        return table is null ? null : MapToDto(table);
    }

    public async Task<TableDto> CreateTableAsync(
        Guid tenantId,
        Guid branchId,
        string tableNumber,
        double positionX,
        double positionY,
        double width,
        double height,
        CancellationToken cancellationToken = default)
    {
        // Check table count limit
        var currentCount = await GetTableCountAsync(tenantId, branchId, cancellationToken);
        if (currentCount >= MaxTablesPerBranch)
        {
            throw new InvalidOperationException($"Cannot create more than {MaxTablesPerBranch} tables per branch.");
        }

        // Check for duplicate table number within branch
        var exists = await context.Tables
            .AnyAsync(t => t.TenantId == tenantId && t.BranchId == branchId && t.TableNumber == tableNumber, cancellationToken);
        if (exists)
        {
            throw new InvalidOperationException($"Table number '{tableNumber}' already exists in this branch.");
        }

        var table = new Table(tenantId, branchId, tableNumber, positionX, positionY, width, height);
        context.Tables.Add(table);
        await context.SaveChangesAsync(cancellationToken);

        return MapToDto(table);
    }

    public async Task<IEnumerable<TableDto>> BulkCreateTablesAsync(
        Guid tenantId,
        Guid branchId,
        int count,
        CancellationToken cancellationToken = default)
    {
        if (count < MinTablesPerBranch)
        {
            throw new InvalidOperationException($"Minimum {MinTablesPerBranch} tables required per branch.");
        }

        if (count > MaxTablesPerBranch)
        {
            throw new InvalidOperationException($"Cannot create more than {MaxTablesPerBranch} tables per branch.");
        }

        // Check current table count
        var currentCount = await GetTableCountAsync(tenantId, branchId, cancellationToken);
        if (currentCount + count > MaxTablesPerBranch)
        {
            throw new InvalidOperationException(
                $"Cannot create {count} tables. Branch already has {currentCount} tables. Maximum is {MaxTablesPerBranch}.");
        }

        // Get existing table numbers to avoid duplicates
        var existingNumbers = await context.Tables
            .Where(t => t.TenantId == tenantId && t.BranchId == branchId)
            .Select(t => t.TableNumber)
            .ToListAsync(cancellationToken);

        var tables = new List<Table>();
        var tableNumber = 1;
        var created = 0;

        // Calculate grid layout (e.g., 5 columns)
        const int columns = 5;
        const double spacing = 120;
        const double startX = 50;
        const double startY = 50;

        while (created < count)
        {
            var numberStr = tableNumber.ToString();
            if (!existingNumbers.Contains(numberStr))
            {
                var row = created / columns;
                var col = created % columns;
                var posX = startX + (col * spacing);
                var posY = startY + (row * spacing);

                var table = new Table(tenantId, branchId, numberStr, posX, posY);
                tables.Add(table);
                created++;
            }
            tableNumber++;

            // Safety check to prevent infinite loop
            if (tableNumber > 1000) break;
        }

        context.Tables.AddRange(tables);
        await context.SaveChangesAsync(cancellationToken);

        return tables.Select(MapToDto);
    }

    public async Task<TableDto> UpdateTableAsync(
        Guid tableId,
        Guid tenantId,
        string tableNumber,
        double positionX,
        double positionY,
        double width,
        double height,
        CancellationToken cancellationToken = default)
    {
        var table = await context.Tables
            .FirstOrDefaultAsync(t => t.Id == tableId && t.TenantId == tenantId, cancellationToken);

        if (table is null)
        {
            throw new InvalidOperationException("Table not found.");
        }

        // Check for duplicate table number if changed
        if (table.TableNumber != tableNumber)
        {
            var exists = await context.Tables
                .AnyAsync(t => t.TenantId == tenantId && t.BranchId == table.BranchId 
                    && t.TableNumber == tableNumber && t.Id != tableId, cancellationToken);
            if (exists)
            {
                throw new InvalidOperationException($"Table number '{tableNumber}' already exists in this branch.");
            }
            table.UpdateTableNumber(tableNumber);
        }

        table.UpdatePosition(positionX, positionY);
        table.UpdateSize(width, height);

        await context.SaveChangesAsync(cancellationToken);

        return MapToDto(table);
    }

    public async Task<TableDto> UpdateTableStatusAsync(
        Guid tableId,
        Guid tenantId,
        Guid? userBranchId,
        TableStatus status,
        CancellationToken cancellationToken = default)
    {
        var query = context.Tables
            .Where(t => t.Id == tableId && t.TenantId == tenantId);

        // Apply branch filter if user is Manager (not Admin)
        if (userBranchId.HasValue)
        {
            query = query.Where(t => t.BranchId == userBranchId.Value);
        }

        var table = await query.FirstOrDefaultAsync(cancellationToken);

        if (table is null)
        {
            throw new InvalidOperationException("Table not found or access denied.");
        }

        table.UpdateStatus(status);
        await context.SaveChangesAsync(cancellationToken);

        return MapToDto(table);
    }

    public async Task DeleteTableAsync(
        Guid tableId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var table = await context.Tables
            .FirstOrDefaultAsync(t => t.Id == tableId && t.TenantId == tenantId, cancellationToken);

        if (table is null)
        {
            throw new InvalidOperationException("Table not found.");
        }

        // Check minimum table count
        var branchTableCount = await GetTableCountAsync(tenantId, table.BranchId, cancellationToken);
        if (branchTableCount <= MinTablesPerBranch)
        {
            throw new InvalidOperationException($"Cannot delete table. Minimum {MinTablesPerBranch} tables required per branch.");
        }

        context.Tables.Remove(table);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> GetTableCountAsync(
        Guid tenantId,
        Guid branchId,
        CancellationToken cancellationToken = default)
    {
        return await context.Tables
            .CountAsync(t => t.TenantId == tenantId && t.BranchId == branchId, cancellationToken);
    }

    private static TableDto MapToDto(Table table)
    {
        return new TableDto(
            table.Id,
            table.BranchId,
            table.TableNumber,
            table.Status,
            table.PositionX,
            table.PositionY,
            table.Width,
            table.Height,
            table.CreatedAt,
            table.UpdatedAt);
    }
}

