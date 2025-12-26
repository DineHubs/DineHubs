using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Branches;
using OrderManagement.Application.Printing;
using OrderManagement.Api.Contracts.Printing;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
public class PrinterConfigurationsController(
    IPrinterConfigurationService printerConfigurationService,
    IBranchService branchService,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        try
        {
            var printers = await printerConfigurationService.GetAllAsync(
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            var branches = await branchService.GetBranchesAsync(tenantContext.TenantId, cancellationToken);
            var branchDict = branches.ToDictionary(b => b.Id, b => b.Name);

            var response = printers.Select(p => new PrinterConfigurationResponse(
                p.Id,
                p.BranchId,
                branchDict.TryGetValue(p.BranchId, out var name) ? name : "Unknown",
                p.Name,
                p.Type,
                p.ConnectionType,
                p.PrinterName,
                p.PaperWidth,
                p.IsDefault,
                p.IsActive,
                p.CreatedAt,
                p.UpdatedAt));

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving printer configurations");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving printer configurations.");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var printer = await printerConfigurationService.GetByIdAsync(
                id,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (printer is null)
            {
                return NotFound(new { Message = "Printer configuration not found." });
            }

            var branches = await branchService.GetBranchesAsync(tenantContext.TenantId, cancellationToken);
            var branchName = branches.FirstOrDefault(b => b.Id == printer.BranchId)?.Name ?? "Unknown";

            var response = new PrinterConfigurationResponse(
                printer.Id,
                printer.BranchId,
                branchName,
                printer.Name,
                printer.Type,
                printer.ConnectionType,
                printer.PrinterName,
                printer.PaperWidth,
                printer.IsDefault,
                printer.IsActive,
                printer.CreatedAt,
                printer.UpdatedAt);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving printer configuration {PrinterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the printer configuration.");
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePrinterConfigurationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = new CreatePrinterConfigurationDto(
                request.BranchId,
                request.Name,
                request.Type,
                request.ConnectionType,
                request.PrinterName,
                request.PaperWidth,
                request.IsDefault);

            var printer = await printerConfigurationService.CreateAsync(
                dto,
                tenantContext.TenantId,
                cancellationToken);

            var branches = await branchService.GetBranchesAsync(tenantContext.TenantId, cancellationToken);
            var branchName = branches.FirstOrDefault(b => b.Id == printer.BranchId)?.Name ?? "Unknown";

            var response = new PrinterConfigurationResponse(
                printer.Id,
                printer.BranchId,
                branchName,
                printer.Name,
                printer.Type,
                printer.ConnectionType,
                printer.PrinterName,
                printer.PaperWidth,
                printer.IsDefault,
                printer.IsActive,
                printer.CreatedAt,
                printer.UpdatedAt);

            return CreatedAtAction(nameof(GetById), new { id = printer.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error creating printer configuration: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error creating printer configuration");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the printer configuration.");
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePrinterConfigurationRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var dto = new UpdatePrinterConfigurationDto(
                request.Name,
                request.Type,
                request.ConnectionType,
                request.PrinterName,
                request.PaperWidth,
                request.IsDefault,
                request.IsActive);

            var printer = await printerConfigurationService.UpdateAsync(
                id,
                dto,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            var branches = await branchService.GetBranchesAsync(tenantContext.TenantId, cancellationToken);
            var branchName = branches.FirstOrDefault(b => b.Id == printer.BranchId)?.Name ?? "Unknown";

            var response = new PrinterConfigurationResponse(
                printer.Id,
                printer.BranchId,
                branchName,
                printer.Name,
                printer.Type,
                printer.ConnectionType,
                printer.PrinterName,
                printer.PaperWidth,
                printer.IsDefault,
                printer.IsActive,
                printer.CreatedAt,
                printer.UpdatedAt);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error updating printer configuration {PrinterId}: {Message}", id, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error updating printer configuration {PrinterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while updating the printer configuration.");
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await printerConfigurationService.DeleteAsync(
                id,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error deleting printer configuration {PrinterId}: {Message}", id, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error deleting printer configuration {PrinterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while deleting the printer configuration.");
        }
    }

    [HttpPost("{id:guid}/test")]
    public async Task<IActionResult> TestPrinter(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var result = await printerConfigurationService.TestPrinterAsync(
                id,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (result.Success)
            {
                return Ok(new { result.Message });
            }

            return BadRequest(new { result.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error testing printer {PrinterId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while testing the printer.");
        }
    }
}

