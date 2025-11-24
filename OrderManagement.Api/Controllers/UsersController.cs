using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.Abstractions;
using OrderManagement.Application.Users;
using OrderManagement.Domain.Identity;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = $"{SystemRoles.SuperAdmin},{SystemRoles.Admin}")]
public class UsersController(
    IUserService userService,
    ITenantContext tenantContext) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateUserAsync(tenantContext.TenantId, request.Email, request.Password, request.Role, request.BranchId, cancellationToken);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new { user.Id, user.Email });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        var users = await userService.GetUsersAsync(tenantContext.TenantId, cancellationToken);
        return Ok(users);
    }
}

public record CreateUserRequest(string Email, string Password, string Role, Guid? BranchId);
