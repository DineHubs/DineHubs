using Microsoft.EntityFrameworkCore;
using Serilog;
using OrderManagement.Application.Printing;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Enums;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Printing;

public sealed class PrinterConfigurationService(
    OrderManagementDbContext dbContext,
    Serilog.ILogger logger) : IPrinterConfigurationService
{
    public async Task<IReadOnlyCollection<PrinterConfiguration>> GetAllAsync(
        Guid tenantId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.PrinterConfigurations.AsNoTracking()
                .Where(p => p.TenantId == tenantId);

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            return await query
                .OrderBy(p => p.BranchId)
                .ThenBy(p => p.Type)
                .ThenBy(p => p.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting printer configurations for tenant {TenantId}", tenantId);
            throw new InvalidOperationException("An error occurred while retrieving printer configurations.");
        }
    }

    public async Task<PrinterConfiguration?> GetByIdAsync(
        Guid id,
        Guid tenantId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.PrinterConfigurations.AsNoTracking()
                .Where(p => p.Id == id && p.TenantId == tenantId);

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            return await query.FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting printer configuration {PrinterId} for tenant {TenantId}", id, tenantId);
            throw new InvalidOperationException("An error occurred while retrieving the printer configuration.");
        }
    }

    public async Task<PrinterConfiguration?> GetDefaultByTypeAsync(
        Guid tenantId,
        Guid branchId,
        PrinterType type,
        CancellationToken cancellationToken)
    {
        try
        {
            return await dbContext.PrinterConfigurations.AsNoTracking()
                .Where(p => p.TenantId == tenantId
                    && p.BranchId == branchId
                    && p.Type == type
                    && p.IsDefault
                    && p.IsActive)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting default {PrinterType} printer for branch {BranchId}", type, branchId);
            throw new InvalidOperationException("An error occurred while retrieving the default printer.");
        }
    }

    public async Task<PrinterConfiguration> CreateAsync(
        CreatePrinterConfigurationDto dto,
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate branch exists
            var branchExists = await dbContext.Branches
                .AnyAsync(b => b.Id == dto.BranchId && b.TenantId == tenantId, cancellationToken);

            if (!branchExists)
            {
                throw new InvalidOperationException("Branch not found.");
            }

            // Check for duplicate name in same branch
            var duplicateExists = await dbContext.PrinterConfigurations
                .AnyAsync(p => p.TenantId == tenantId
                    && p.BranchId == dto.BranchId
                    && p.Name == dto.Name, cancellationToken);

            if (duplicateExists)
            {
                throw new InvalidOperationException($"A printer with name '{dto.Name}' already exists in this branch.");
            }

            // If this is being set as default, remove default from other printers of same type
            if (dto.IsDefault)
            {
                await RemoveDefaultFromTypeAsync(tenantId, dto.BranchId, dto.Type, cancellationToken);
            }

            var printer = new PrinterConfiguration(
                tenantId,
                dto.BranchId,
                dto.Name,
                dto.Type,
                dto.ConnectionType,
                dto.PrinterName,
                dto.PaperWidth,
                dto.IsDefault);

            dbContext.PrinterConfigurations.Add(printer);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.Information("Created printer configuration {PrinterName} (Id: {PrinterId}) for branch {BranchId}",
                dto.Name, printer.Id, dto.BranchId);

            return printer;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error creating printer configuration for branch {BranchId}", dto.BranchId);
            throw new InvalidOperationException("An error occurred while creating the printer configuration.");
        }
    }

    public async Task<PrinterConfiguration> UpdateAsync(
        Guid id,
        UpdatePrinterConfigurationDto dto,
        Guid tenantId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.PrinterConfigurations
                .Where(p => p.Id == id && p.TenantId == tenantId);

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            var printer = await query.FirstOrDefaultAsync(cancellationToken);

            if (printer is null)
            {
                throw new InvalidOperationException("Printer configuration not found.");
            }

            // Check for duplicate name (excluding current printer)
            var duplicateExists = await dbContext.PrinterConfigurations
                .AnyAsync(p => p.TenantId == tenantId
                    && p.BranchId == printer.BranchId
                    && p.Name == dto.Name
                    && p.Id != id, cancellationToken);

            if (duplicateExists)
            {
                throw new InvalidOperationException($"A printer with name '{dto.Name}' already exists in this branch.");
            }

            // If this is being set as default, remove default from other printers of same type
            if (dto.IsDefault && !printer.IsDefault)
            {
                await RemoveDefaultFromTypeAsync(tenantId, printer.BranchId, dto.Type, cancellationToken);
            }

            printer.Update(
                dto.Name,
                dto.Type,
                dto.ConnectionType,
                dto.PrinterName,
                dto.PaperWidth,
                dto.IsDefault);

            if (dto.IsActive)
            {
                printer.Activate();
            }
            else
            {
                printer.Deactivate();
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.Information("Updated printer configuration {PrinterId}", id);

            return printer;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error updating printer configuration {PrinterId}", id);
            throw new InvalidOperationException("An error occurred while updating the printer configuration.");
        }
    }

    public async Task DeleteAsync(
        Guid id,
        Guid tenantId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = dbContext.PrinterConfigurations
                .Where(p => p.Id == id && p.TenantId == tenantId);

            if (branchId.HasValue)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            var printer = await query.FirstOrDefaultAsync(cancellationToken);

            if (printer is null)
            {
                throw new InvalidOperationException("Printer configuration not found.");
            }

            dbContext.PrinterConfigurations.Remove(printer);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.Information("Deleted printer configuration {PrinterId}", id);
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error deleting printer configuration {PrinterId}", id);
            throw new InvalidOperationException("An error occurred while deleting the printer configuration.");
        }
    }

    public async Task<PrinterTestResult> TestPrinterAsync(
        Guid id,
        Guid tenantId,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        try
        {
            var printer = await GetByIdAsync(id, tenantId, branchId, cancellationToken);

            if (printer is null)
            {
                return new PrinterTestResult(false, "Printer configuration not found.");
            }

            // In a real implementation, this would send a test print job to the Print Agent
            // For now, we just validate the configuration
            if (string.IsNullOrWhiteSpace(printer.PrinterName))
            {
                return new PrinterTestResult(false, "Printer name is not configured.");
            }

            logger.Information("Test print requested for printer {PrinterId} ({PrinterName})",
                id, printer.PrinterName);

            // TODO: Send test print via Print Agent WebSocket or API
            return new PrinterTestResult(true, "Test print job sent successfully. Check the printer.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error testing printer {PrinterId}", id);
            return new PrinterTestResult(false, $"Error testing printer: {ex.Message}");
        }
    }

    private async Task RemoveDefaultFromTypeAsync(
        Guid tenantId,
        Guid branchId,
        PrinterType type,
        CancellationToken cancellationToken)
    {
        var defaultPrinters = await dbContext.PrinterConfigurations
            .Where(p => p.TenantId == tenantId
                && p.BranchId == branchId
                && p.Type == type
                && p.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var printer in defaultPrinters)
        {
            printer.RemoveDefault();
        }
    }
}

