using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Contracts.Tables;

public record TableResponse(
    Guid Id,
    Guid BranchId,
    string TableNumber,
    TableStatus Status,
    string StatusName,
    double PositionX,
    double PositionY,
    double Width,
    double Height,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

