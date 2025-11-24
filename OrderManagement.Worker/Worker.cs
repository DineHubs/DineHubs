using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OrderManagement.Application.Subscriptions;
using OrderManagement.Infrastructure.Persistence;

namespace OrderManagement.Worker;

public class UsageTrackingWorker : BackgroundService
{
    private readonly ILogger<UsageTrackingWorker> _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TimeSpan _delay;

    public UsageTrackingWorker(
        ILogger<UsageTrackingWorker> logger,
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        var intervalSeconds = configuration.GetValue("Worker:PollingIntervalSeconds", 30);
        _delay = TimeSpan.FromSeconds(intervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<OrderManagementDbContext>();
                var usageTracker = scope.ServiceProvider.GetRequiredService<IUsageTracker>();

                var tenantIds = await dbContext.Tenants.AsNoTracking()
                    .Select(t => t.Id)
                    .ToListAsync(stoppingToken);

                foreach (var tenantId in tenantIds)
                {
                    await usageTracker.CaptureAsync(tenantId, stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to capture tenant usage.");
            }

            await Task.Delay(_delay, stoppingToken);
        }
    }
}
