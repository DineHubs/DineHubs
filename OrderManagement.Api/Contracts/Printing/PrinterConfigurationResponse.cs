using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Contracts.Printing;

public record PrinterConfigurationResponse(
    Guid Id,
    Guid BranchId,
    string BranchName,
    string Name,
    PrinterType Type,
    ConnectionType ConnectionType,
    string PrinterName,
    int PaperWidth,
    bool IsDefault,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

