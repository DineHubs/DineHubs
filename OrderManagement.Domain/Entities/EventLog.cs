namespace OrderManagement.Domain.Entities;

public class EventLog
{
    public int Id { get; private set; }
    public string LogLevel { get; private set; } = string.Empty;
    public string Message { get; private set; } = string.Empty;
    public string? Exception { get; private set; }
    public string? Properties { get; private set; } // JSON string
    public string? Source { get; private set; }
    public Guid? TenantId { get; private set; }
    public Guid? UserId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;
    public Guid? CreatedBy { get; private set; }
    public DateTimeOffset? UpdatedAt { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    private EventLog()
    {
    }

    public EventLog(
        string logLevel,
        string message,
        string? exception = null,
        string? properties = null,
        string? source = null,
        Guid? tenantId = null,
        Guid? userId = null)
    {
        LogLevel = logLevel;
        Message = message;
        Exception = exception;
        Properties = properties;
        Source = source;
        TenantId = tenantId;
        UserId = userId;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}

