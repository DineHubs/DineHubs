using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Tables;

public interface ITableService
{
    /// <summary>
    /// Gets all tables for a branch. If userBranchId is set, only returns tables for that branch.
    /// </summary>
    Task<IEnumerable<TableDto>> GetTablesAsync(
        Guid tenantId,
        Guid? userBranchId,
        Guid? requestedBranchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a table by ID with branch access validation.
    /// </summary>
    Task<TableDto?> GetTableByIdAsync(
        Guid tableId,
        Guid tenantId,
        Guid? userBranchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new table for a branch. Admin only.
    /// </summary>
    Task<TableDto> CreateTableAsync(
        Guid tenantId,
        Guid branchId,
        string tableNumber,
        double positionX,
        double positionY,
        double width,
        double height,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates multiple tables for a branch. Admin only.
    /// </summary>
    Task<IEnumerable<TableDto>> BulkCreateTablesAsync(
        Guid tenantId,
        Guid branchId,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a table's properties. Admin only.
    /// </summary>
    Task<TableDto> UpdateTableAsync(
        Guid tableId,
        Guid tenantId,
        string tableNumber,
        double positionX,
        double positionY,
        double width,
        double height,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a table's status. Admin and Manager only.
    /// </summary>
    Task<TableDto> UpdateTableStatusAsync(
        Guid tableId,
        Guid tenantId,
        Guid? userBranchId,
        TableStatus status,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a table. Admin only.
    /// </summary>
    Task DeleteTableAsync(
        Guid tableId,
        Guid tenantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the table count for a branch.
    /// </summary>
    Task<int> GetTableCountAsync(
        Guid tenantId,
        Guid branchId,
        CancellationToken cancellationToken = default);
}

public record TableDto(
    Guid Id,
    Guid BranchId,
    string TableNumber,
    TableStatus Status,
    double PositionX,
    double PositionY,
    double Width,
    double Height,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

