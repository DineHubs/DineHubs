using Microsoft.AspNetCore.Http;

namespace OrderManagement.Api.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;

    public SecurityHeadersMiddleware(RequestDelegate next, IWebHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Remove server header
        context.Response.Headers.Remove("Server");

        // X-Content-Type-Options: Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // X-Frame-Options: Prevent clickjacking
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // X-XSS-Protection: Enable XSS filter (legacy browsers)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Referrer-Policy: Control referrer information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Permissions-Policy: Control browser features
        context.Response.Headers.Append("Permissions-Policy", 
            "geolocation=(), microphone=(), camera=(), payment=(), usb=(), magnetometer=(), gyroscope=(), accelerometer=()");

        // Content-Security-Policy: Prevent XSS attacks
        // Note: For Angular apps, if inline scripts/styles are required, use nonces or hashes instead
        var csp = "default-src 'self'; " +
                  "script-src 'self'; " +
                  "style-src 'self'; " +
                  "img-src 'self' data: https:; " +
                  "font-src 'self' data:; " +
                  "connect-src 'self'; " +
                  "frame-ancestors 'none'; " +
                  "base-uri 'self'; " +
                  "form-action 'self';";
        
        // In development, allow unsafe-inline for easier debugging (remove in production)
        if (_environment.IsDevelopment())
        {
            csp = "default-src 'self'; " +
                  "script-src 'self' 'unsafe-inline'; " +
                  "style-src 'self' 'unsafe-inline'; " +
                  "img-src 'self' data: https:; " +
                  "font-src 'self' data:; " +
                  "connect-src 'self'; " +
                  "frame-ancestors 'none'; " +
                  "base-uri 'self'; " +
                  "form-action 'self';";
        }
        
        context.Response.Headers.Append("Content-Security-Policy", csp);

        // Strict-Transport-Security (HSTS): Force HTTPS (production only)
        if (_environment.IsProduction())
        {
            context.Response.Headers.Append("Strict-Transport-Security", 
                "max-age=31536000; includeSubDomains; preload");
        }

        await _next(context);
    }
}

