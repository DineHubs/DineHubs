using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;

namespace OrderManagement.Application.Printing;

public interface IPrinterConfigurationService
{
    Task<IReadOnlyCollection<PrinterConfiguration>> GetAllAsync(
        Guid tenantId, 
        Guid? branchId, 
        CancellationToken cancellationToken);

    Task<PrinterConfiguration?> GetByIdAsync(
        Guid id, 
        Guid tenantId, 
        Guid? branchId, 
        CancellationToken cancellationToken);

    Task<PrinterConfiguration?> GetDefaultByTypeAsync(
        Guid tenantId, 
        Guid branchId, 
        PrinterType type, 
        CancellationToken cancellationToken);

    Task<PrinterConfiguration> CreateAsync(
        CreatePrinterConfigurationDto dto, 
        Guid tenantId, 
        CancellationToken cancellationToken);

    Task<PrinterConfiguration> UpdateAsync(
        Guid id, 
        UpdatePrinterConfigurationDto dto, 
        Guid tenantId, 
        Guid? branchId, 
        CancellationToken cancellationToken);

    Task DeleteAsync(
        Guid id, 
        Guid tenantId, 
        Guid? branchId, 
        CancellationToken cancellationToken);

    Task<PrinterTestResult> TestPrinterAsync(
        Guid id, 
        Guid tenantId, 
        Guid? branchId, 
        CancellationToken cancellationToken);
}

public record CreatePrinterConfigurationDto(
    Guid BranchId,
    string Name,
    PrinterType Type,
    ConnectionType ConnectionType,
    string PrinterName,
    int PaperWidth = 80,
    bool IsDefault = false);

public record UpdatePrinterConfigurationDto(
    string Name,
    PrinterType Type,
    ConnectionType ConnectionType,
    string PrinterName,
    int PaperWidth,
    bool IsDefault,
    bool IsActive);

public record PrinterTestResult(
    bool Success,
    string Message);

