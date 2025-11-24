using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using Microsoft.Extensions.DependencyInjection;

namespace OrderManagement.Infrastructure.Logging;

public sealed class BatchingDatabaseSink : IBatchedLogEventSink
{
    private readonly DatabaseSink _databaseSink;

    public BatchingDatabaseSink(IServiceProvider serviceProvider, int batchSizeLimit = 50, TimeSpan? period = null)
    {
        _databaseSink = new DatabaseSink(serviceProvider);
    }

    public Task EmitBatchAsync(IEnumerable<LogEvent> batch)
    {
        foreach (var logEvent in batch)
        {
            _databaseSink.Emit(logEvent);
        }
        return Task.CompletedTask;
    }

    public Task OnEmptyBatchAsync()
    {
        return Task.CompletedTask;
    }
}

