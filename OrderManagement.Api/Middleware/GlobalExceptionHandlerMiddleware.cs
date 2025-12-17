using System.Net;
using System.Text.Json;
using FluentValidation;
using Serilog;
using ILogger = Serilog.ILogger;

namespace OrderManagement.Api.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        _next = next;
        _environment = environment;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;

        var errorResponse = new ErrorResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Message = "An error occurred while processing your request."
        };

        switch (exception)
        {
            case ValidationException validationEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Validation failed.";
                errorResponse.Details = string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage));
                _logger.Warning(exception, "Validation failed: {Errors}", string.Join("; ", validationEx.Errors.Select(e => e.ErrorMessage)));
                break;

            case InvalidOperationException invalidOpEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = invalidOpEx.Message;
                _logger.Warning(exception, "Invalid operation: {Message}", invalidOpEx.Message);
                break;

            case UnauthorizedAccessException:
                response.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.StatusCode = (int)HttpStatusCode.Unauthorized;
                errorResponse.Message = "Unauthorized access.";
                _logger.Warning(exception, "Unauthorized access attempt");
                break;

            case KeyNotFoundException keyNotFoundEx:
                response.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.StatusCode = (int)HttpStatusCode.NotFound;
                errorResponse.Message = keyNotFoundEx.Message;
                _logger.Warning(exception, "Resource not found: {Message}", keyNotFoundEx.Message);
                break;

            case ArgumentException argEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = argEx.Message;
                _logger.Warning(exception, "Invalid argument: {Message}", argEx.Message);
                break;

            case FormatException formatEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Invalid data format. Please check that all IDs are valid GUIDs.";
                _logger.Warning(exception, "Format error (likely invalid GUID): {Message}", formatEx.Message);
                break;

            case System.Text.Json.JsonException jsonEx:
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.StatusCode = (int)HttpStatusCode.BadRequest;
                errorResponse.Message = "Invalid JSON format. Please check the request data.";
                _logger.Warning(exception, "JSON deserialization error: {Message}", jsonEx.Message);
                break;

            default:
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.Message = _environment.IsDevelopment()
                    ? exception.Message
                    : "An error occurred while processing your request.";
                
                if (_environment.IsDevelopment())
                {
                    errorResponse.Details = exception.ToString();
                }
                
                _logger.Error(exception, "Unhandled exception occurred");
                break;
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        var jsonResponse = JsonSerializer.Serialize(errorResponse, options);
        await response.WriteAsync(jsonResponse);
    }

    private sealed class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}

