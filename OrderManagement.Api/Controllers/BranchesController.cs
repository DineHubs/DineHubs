using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Branches;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = SystemRoles.Admin)]
public class BranchesController(
    IBranchService branchService,
    ITenantContext tenantContext,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateBranch([FromBody] CreateBranchRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var branch = await branchService.CreateBranchAsync(tenantContext.TenantId, request.Name, request.Location, cancellationToken);
            return CreatedAtAction(nameof(GetBranches), new { id = branch.Id }, branch);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error creating branch: {Message}", ex.Message);
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error creating branch");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the branch.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBranches(CancellationToken cancellationToken)
    {
        try
        {
            var branches = await branchService.GetBranchesAsync(tenantContext.TenantId, cancellationToken);
            return Ok(branches);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving branches");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving branches.");
        }
    }
}

public record CreateBranchRequest(string Name, string Location);
