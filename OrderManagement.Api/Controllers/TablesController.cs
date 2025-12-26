using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Api.Contracts.Tables;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Tables;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class TablesController(
    ITableService tableService,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : ControllerBase
{
    /// <summary>
    /// Get all tables for the current branch (or specified branch for Admin)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> GetTables([FromQuery] Guid? branchId, CancellationToken cancellationToken)
    {
        try
        {
            // If user is Manager/Waiter, they have a BranchId set and can only see their branch
            // If user is Admin, they can specify a branchId or see all tables
            var tables = await tableService.GetTablesAsync(
                tenantContext.TenantId,
                tenantContext.BranchId,
                branchId,
                cancellationToken);

            var response = tables.Select(t => new TableResponse(
                t.Id,
                t.BranchId,
                t.TableNumber,
                t.Status,
                t.Status.ToString(),
                t.PositionX,
                t.PositionY,
                t.Width,
                t.Height,
                t.CreatedAt,
                t.UpdatedAt));

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving tables for tenant {TenantId}", tenantContext.TenantId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while retrieving tables." });
        }
    }

    /// <summary>
    /// Get a specific table by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager},{SystemRoles.Waiter}")]
    public async Task<IActionResult> GetTable(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var table = await tableService.GetTableByIdAsync(
                id,
                tenantContext.TenantId,
                tenantContext.BranchId,
                cancellationToken);

            if (table is null)
            {
                return NotFound(new { Message = "Table not found or access denied." });
            }

            var response = new TableResponse(
                table.Id,
                table.BranchId,
                table.TableNumber,
                table.Status,
                table.Status.ToString(),
                table.PositionX,
                table.PositionY,
                table.Width,
                table.Height,
                table.CreatedAt,
                table.UpdatedAt);

            return Ok(response);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving table {TableId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while retrieving the table." });
        }
    }

    /// <summary>
    /// Create a new table (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> CreateTable([FromBody] CreateTableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var table = await tableService.CreateTableAsync(
                tenantContext.TenantId,
                request.BranchId,
                request.TableNumber,
                request.PositionX,
                request.PositionY,
                request.Width,
                request.Height,
                cancellationToken);

            var response = new TableResponse(
                table.Id,
                table.BranchId,
                table.TableNumber,
                table.Status,
                table.Status.ToString(),
                table.PositionX,
                table.PositionY,
                table.Width,
                table.Height,
                table.CreatedAt,
                table.UpdatedAt);

            return CreatedAtAction(nameof(GetTable), new { id = table.Id }, response);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error creating table: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error creating table");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while creating the table." });
        }
    }

    /// <summary>
    /// Bulk create tables for a branch (Admin only)
    /// </summary>
    [HttpPost("bulk")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> BulkCreateTables([FromBody] BulkCreateTablesRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tables = await tableService.BulkCreateTablesAsync(
                tenantContext.TenantId,
                request.BranchId,
                request.Count,
                cancellationToken);

            var response = tables.Select(t => new TableResponse(
                t.Id,
                t.BranchId,
                t.TableNumber,
                t.Status,
                t.Status.ToString(),
                t.PositionX,
                t.PositionY,
                t.Width,
                t.Height,
                t.CreatedAt,
                t.UpdatedAt));

            return CreatedAtAction(nameof(GetTables), response);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error bulk creating tables: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error bulk creating tables");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while creating tables." });
        }
    }

    /// <summary>
    /// Update a table (Admin only)
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> UpdateTable(Guid id, [FromBody] UpdateTableRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var table = await tableService.UpdateTableAsync(
                id,
                tenantContext.TenantId,
                request.TableNumber,
                request.PositionX,
                request.PositionY,
                request.Width,
                request.Height,
                cancellationToken);

            var response = new TableResponse(
                table.Id,
                table.BranchId,
                table.TableNumber,
                table.Status,
                table.Status.ToString(),
                table.PositionX,
                table.PositionY,
                table.Width,
                table.Height,
                table.CreatedAt,
                table.UpdatedAt);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error updating table {TableId}: {Message}", id, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error updating table {TableId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while updating the table." });
        }
    }

    /// <summary>
    /// Update table status (Admin and Manager only - Waiter is view-only)
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Manager}")]
    public async Task<IActionResult> UpdateTableStatus(Guid id, [FromBody] UpdateTableStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var table = await tableService.UpdateTableStatusAsync(
                id,
                tenantContext.TenantId,
                tenantContext.BranchId,
                request.Status,
                cancellationToken);

            var response = new TableResponse(
                table.Id,
                table.BranchId,
                table.TableNumber,
                table.Status,
                table.Status.ToString(),
                table.PositionX,
                table.PositionY,
                table.Width,
                table.Height,
                table.CreatedAt,
                table.UpdatedAt);

            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error updating table status {TableId}: {Message}", id, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error updating table status {TableId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while updating the table status." });
        }
    }

    /// <summary>
    /// Delete a table (Admin only)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> DeleteTable(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            await tableService.DeleteTableAsync(
                id,
                tenantContext.TenantId,
                cancellationToken);

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error deleting table {TableId}: {Message}", id, ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error deleting table {TableId}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while deleting the table." });
        }
    }

    /// <summary>
    /// Get table count for a branch (Admin only)
    /// </summary>
    [HttpGet("count")]
    [Authorize(Roles = SystemRoles.Admin)]
    public async Task<IActionResult> GetTableCount([FromQuery] Guid branchId, CancellationToken cancellationToken)
    {
        try
        {
            var count = await tableService.GetTableCountAsync(
                tenantContext.TenantId,
                branchId,
                cancellationToken);

            return Ok(new { BranchId = branchId, Count = count });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving table count for branch {BranchId}", branchId);
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { Message = "An error occurred while retrieving table count." });
        }
    }
}

