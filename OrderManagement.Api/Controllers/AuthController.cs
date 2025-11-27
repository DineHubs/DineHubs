using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using OrderManagement.Api.Contracts.Auth;
using OrderManagement.Application.Auth;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(
    IAuthService authService,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.LoginAsync(request.Email, request.Password, cancellationToken);
            return Ok(new { AccessToken = result.AccessToken, Roles = result.Roles });
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Login failed for {Email}: {Message}", request.Email, ex.Message);
            return Unauthorized(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error during login for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to process login at this time.");
        }
    }

    [HttpPost("seed-super-admin")]
    [AllowAnonymous]
    public async Task<IActionResult> SeedSuperAdmin([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.SeedSuperAdminAsync(request.Email, request.Password, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("already exists"))
            {
                return Conflict(new { Message = ex.Message });
            }
            return BadRequest(new { Message = ex.Message });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error seeding super admin for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to seed super admin at this time.");
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.ForgotPasswordAsync(request.Email, cancellationToken);
            return Ok(new { Message = result.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Forgot password failed for {Email}: {Message}", request.Email, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error processing forgot password for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to process password reset request at this time.");
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword, cancellationToken);
            if (result.Success)
            {
                return Ok(new { Message = result.Message });
            }
            return BadRequest(new { Message = result.Message });
        }
        catch (InvalidOperationException ex)
        {
            logger.Warning("Reset password failed for {Email}: {Message}", request.Email, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Unexpected error resetting password for {Email}", request.Email);
            return StatusCode(StatusCodes.Status500InternalServerError, "Unable to reset password at this time.");
        }
    }
}

