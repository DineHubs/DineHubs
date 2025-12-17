using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.Application.KPIs;
using OrderManagement.Domain.Identity;
using Serilog;

namespace OrderManagement.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
public class KPIsController(
    IKpiService kpiService,
    Serilog.ILogger logger) : ControllerBase
{
    [HttpGet("prep-time")]
    [Authorize(Roles = $"{SystemRoles.Manager}")]
    public async Task<IActionResult> GetAveragePrepTime(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        try
        {
            var averageMinutes = await kpiService.GetAveragePrepTimeAsync(from, to, cancellationToken);
            return Ok(new { AveragePrepTimeMinutes = averageMinutes });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving average prep time");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the average prep time.");
        }
    }

    [HttpGet("order-accuracy")]
    [Authorize(Roles = $"{SystemRoles.Manager}")]
    public async Task<IActionResult> GetOrderAccuracyRate(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        try
        {
            var accuracyRate = await kpiService.GetOrderAccuracyRateAsync(from, to, cancellationToken);
            return Ok(new { AccuracyRate = accuracyRate });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving order accuracy rate");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the order accuracy rate.");
        }
    }

    [HttpGet("table-turn-time")]
    [Authorize(Roles = $"{SystemRoles.Manager}")]
    public async Task<IActionResult> GetAverageTableTurnTime(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        try
        {
            var averageMinutes = await kpiService.GetAverageTableTurnTimeAsync(from, to, cancellationToken);
            return Ok(new { AverageTableTurnTimeMinutes = averageMinutes });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving average table turn time");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the average table turn time.");
        }
    }

    [HttpGet("refund-frequency")]
    [Authorize(Roles = $"{SystemRoles.Manager}")]
    public async Task<IActionResult> GetRefundFrequency(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        try
        {
            var count = await kpiService.GetRefundFrequencyAsync(from, to, cancellationToken);
            return Ok(new { RefundCount = count });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving refund frequency");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the refund frequency.");
        }
    }

    [HttpGet("reprint-count")]
    [Authorize(Roles = $"{SystemRoles.Manager}")]
    public async Task<IActionResult> GetReprintCount(
        [FromQuery] DateTimeOffset from,
        [FromQuery] DateTimeOffset to,
        CancellationToken cancellationToken)
    {
        try
        {
            var count = await kpiService.GetReprintCountAsync(from, to, cancellationToken);
            return Ok(new { ReprintCount = count });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error retrieving reprint count");
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while retrieving the reprint count.");
        }
    }

    [HttpGet("orders/{orderId:guid}/prep-time")]
    [Authorize(Roles = $"{SystemRoles.Manager},{SystemRoles.Kitchen}")]
    public async Task<IActionResult> GetOrderPrepTime(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var prepTime = await kpiService.CalculatePrepTimeAsync(orderId, cancellationToken);
            if (!prepTime.HasValue)
            {
                return NotFound(new { Message = "Prep time not available for this order." });
            }
            return Ok(new { PrepTimeMinutes = prepTime.Value.TotalMinutes });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error calculating prep time for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while calculating the prep time.");
        }
    }

    [HttpGet("orders/{orderId:guid}/table-turn-time")]
    [Authorize(Roles = $"{SystemRoles.Manager}")]
    public async Task<IActionResult> GetOrderTableTurnTime(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        try
        {
            var turnTime = await kpiService.CalculateTableTurnTimeAsync(orderId, cancellationToken);
            if (!turnTime.HasValue)
            {
                return NotFound(new { Message = "Table turn time not available for this order." });
            }
            return Ok(new { TableTurnTimeMinutes = turnTime.Value.TotalMinutes });
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error calculating table turn time for order {OrderId}", orderId);
            return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while calculating the table turn time.");
        }
    }
}

