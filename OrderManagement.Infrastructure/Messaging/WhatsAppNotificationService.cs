using Serilog;
using Microsoft.Extensions.Options;
using OrderManagement.Application.Abstractions.Notifications;
using OrderManagement.Infrastructure.Options;

namespace OrderManagement.Infrastructure.Messaging;

public sealed class WhatsAppNotificationService(
    Serilog.ILogger logger,
    IOptions<WhatsAppOptions> options) : IWhatsAppNotificationService
{
    private readonly WhatsAppOptions _options = options.Value;

    public Task SendTemplateAsync(string phoneNumber, string templateName, object payload, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.Information("Dispatching WhatsApp template {Template} to {Number} via {Provider}", templateName, phoneNumber, _options.Provider);
            // TODO integrate with WhatsApp Business API
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error sending WhatsApp template {Template} to {Number}", templateName, phoneNumber);
            throw;
        }
    }
}


