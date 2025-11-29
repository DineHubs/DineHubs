using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
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
    ITenantContext tenantContext,
    ICurrentUserContext currentUserContext,
    Serilog.ILogger logger) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Log request details for debugging
            logger.Information("Creating user request - Email: {Email}, Role: {Role}, CurrentUserRoles: {Roles}, TenantId: {TenantId}", 
                request.Email, request.Role, string.Join(", ", currentUserContext.Roles), tenantContext.TenantId);

            var user = await userService.CreateUserAsync(
                tenantContext.TenantId, 
                request.Email, 
                request.Password, 
                request.Role, 
                request.BranchId,
                currentUserContext.Roles,
                cancellationToken);
            
            logger.Information("Successfully created user {Email} with role {Role}", request.Email, request.Role);
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new { user.Id, user.Email });
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error creating user: {Message}. Request: Email={Email}, Role={Role}, CurrentUserRoles={Roles}", 
                ex.Message, request.Email, request.Role, string.Join(", ", currentUserContext.Roles));
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error creating user. Request: Email={Email}, Role={Role}", request.Email, request.Role);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while creating the user.");
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers(CancellationToken cancellationToken)
    {
        try
        {
            var users = await userService.GetUsersAsync(tenantContext.TenantId, cancellationToken);
            return Ok(users);
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Error retrieving users: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error retrieving users");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving users.");
        }
    }
}

public record CreateUserRequest(string Email, string Password, string Role, Guid? BranchId);
