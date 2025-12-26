using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Contracts.Printing;

public record CreatePrinterConfigurationRequest(
    Guid BranchId,
    string Name,
    PrinterType Type,
    ConnectionType ConnectionType,
    string PrinterName,
    int PaperWidth = 80,
    bool IsDefault = false);

