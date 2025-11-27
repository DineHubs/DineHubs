using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Branches;
using OrderManagement.Domain.Entities;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = SystemRoles.Admin)]
public class BranchesController(
    IBranchService branchService,
    ITenantContext tenantContext) : ControllerBase
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
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetBranches(CancellationToken cancellationToken)
    {
        var branches = await branchService.GetBranchesAsync(tenantContext.TenantId, cancellationToken);
        return Ok(branches);
    }
}

public record CreateBranchRequest(string Name, string Location);
