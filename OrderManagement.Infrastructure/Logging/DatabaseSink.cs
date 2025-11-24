using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;
using OrderManagement.Application.Abstractions;
using OrderManagement.Domain.Entities;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Infrastructure.Logging;

public sealed class DatabaseSink : ILogEventSink
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IFormatProvider? _formatProvider;

    public DatabaseSink(IServiceProvider serviceProvider, IFormatProvider? formatProvider = null)
    {
        _serviceProvider = serviceProvider;
        _formatProvider = formatProvider;
    }

    public void Emit(LogEvent logEvent)
    {
        // Only save Errors and Warnings to database
        if (logEvent.Level < LogEventLevel.Warning)
        {
            return;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();

            // Extract tenant and user from log event properties or HttpContext
            var tenantId = ExtractGuidProperty(logEvent, "TenantId");
            var userId = ExtractGuidProperty(logEvent, "UserId");

            // Try to get from HttpContext if not in log event
            var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
            if (httpContextAccessor?.HttpContext != null)
            {
                var tenantContext = scope.ServiceProvider.GetService<ITenantContext>();
                var currentUserContext = scope.ServiceProvider.GetService<ICurrentUserContext>();

                tenantId ??= tenantContext?.TenantId != Guid.Empty ? tenantContext?.TenantId : null;
                userId ??= currentUserContext?.UserId;
            }

            var source = ExtractStringProperty(logEvent, "SourceContext") ?? 
                        ExtractStringProperty(logEvent, "Source");

            // Serialize properties to JSON
            var properties = SerializeProperties(logEvent);

            // Get exception details
            var exception = logEvent.Exception?.ToString();

            // Format message
            var message = logEvent.RenderMessage(_formatProvider);

            var eventLog = new EventLog(
                logLevel: logEvent.Level.ToString(),
                message: message,
                exception: exception,
                properties: properties,
                source: source,
                tenantId: tenantId,
                userId: userId
            );

            dbContext.EventLogs.Add(eventLog);
            dbContext.SaveChanges();
        }
        catch (Exception ex)
        {
            // Prevent infinite loops - use static logger to avoid circular dependencies
            // Use Serilog.Log directly instead of injected logger
            Serilog.Log.Warning(ex, "DatabaseSink failed to write log event to database. This error is not logged to database to prevent infinite loops.");
        }
    }

    private static Guid? ExtractGuidProperty(LogEvent logEvent, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue))
        {
            if (propertyValue is ScalarValue scalar && scalar.Value is Guid guid)
            {
                return guid;
            }
            if (propertyValue is ScalarValue scalarStr && scalarStr.Value is string str && Guid.TryParse(str, out var parsedGuid))
            {
                return parsedGuid;
            }
        }
        return null;
    }

    private static string? ExtractStringProperty(LogEvent logEvent, string propertyName)
    {
        if (logEvent.Properties.TryGetValue(propertyName, out var propertyValue))
        {
            if (propertyValue is ScalarValue scalar && scalar.Value is string str)
            {
                return str;
            }
        }
        return null;
    }

    private static string? SerializeProperties(LogEvent logEvent)
    {
        if (!logEvent.Properties.Any())
        {
            return null;
        }

        try
        {
            var propertiesDict = new Dictionary<string, object?>();
            foreach (var property in logEvent.Properties)
            {
                // Skip internal properties
                if (property.Key.StartsWith("@") || 
                    property.Key == "SourceContext" || 
                    property.Key == "TenantId" || 
                    property.Key == "UserId")
                {
                    continue;
                }

                propertiesDict[property.Key] = property.Value.ToString();
            }

            if (propertiesDict.Count == 0)
            {
                return null;
            }

            return JsonSerializer.Serialize(propertiesDict);
        }
        catch
        {
            return null;
        }
    }
}

