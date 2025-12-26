using OrderManagement.Domain.Enums;

namespace OrderManagement.Api.Contracts.Printing;

public record UpdatePrinterConfigurationRequest(
    string Name,
    PrinterType Type,
    ConnectionType ConnectionType,
    string PrinterName,
    int PaperWidth,
    bool IsDefault,
    bool IsActive);

