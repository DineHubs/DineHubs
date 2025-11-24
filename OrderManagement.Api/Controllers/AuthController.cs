using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Serilog;
using OrderManagement.Api.Contracts.Auth;
using OrderManagement.Application.Auth;
using OrderManagement.Domain.Identity;
using OrderManagement.Identity.Entities;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(
    SignInManager<ApplicationUser> signInManager,
    UserManager<ApplicationUser> userManager,
    IJwtTokenGenerator tokenGenerator,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
            if (!result.Succeeded)
            {
                return Unauthorized();
            }

            var roles = await userManager.GetRolesAsync(user);
            var accessToken = tokenGenerator.GenerateAccessToken(user.Id, user.Email ?? string.Empty, user.TenantId, user.BranchId, roles);
            return Ok(new { AccessToken = accessToken, Roles = roles });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to log in user {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to process login at this time.");
        }
    }

    [HttpPost("seed-super-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedSuperAdmin([FromBody] LoginRequest request)
    {
        try
        {
            var existing = await userManager.FindByEmailAsync(request.Email);
            if (existing is not null)
            {
                return Conflict("Super admin already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                TenantId = Guid.Empty,
                DisplayName = "Super Admin"
            };

            var createResult = await userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }

            await userManager.AddToRoleAsync(user, SystemRoles.SuperAdmin);
            return Ok();
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to seed super admin for email {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to seed super admin at this time.");
        }
    }
}

