using Serilog;
using OrderManagement.Application.Abstractions.Billing;
using OrderManagement.Application.Abstractions.Notifications;
using OrderManagement.Application.Subscriptions.Models;

namespace OrderManagement.Infrastructure.Billing;

public sealed class BillingDispatcher(
    IEmailNotificationService emailNotification,
    IWhatsAppNotificationService whatsAppNotification,
    Serilog.ILogger logger) : IBillingDispatcher
{
    public async Task SendInvoiceAsync(BillingPayload payload, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.Information("Dispatching invoice for tenant {TenantId} via {Channel} to {Recipient}", 
                payload.TenantId, payload.Channel, payload.Recipient);

            if (payload.Channel.Equals("WhatsApp", StringComparison.OrdinalIgnoreCase))
            {
                await whatsAppNotification.SendTemplateAsync(payload.Recipient, "order_invoice", payload, cancellationToken);
            }
            else
            {
                var subject = $"Invoice - {payload.PlanName}";
                var body = $"Amount: {payload.Amount} {payload.Currency}\nCoverage: {payload.PeriodStart:d} - {payload.PeriodEnd:d}";
                await emailNotification.SendAsync(payload.Recipient, subject, body, cancellationToken);
            }

            logger.Information("Successfully dispatched invoice for tenant {TenantId} via {Channel}", 
                payload.TenantId, payload.Channel);
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error dispatching invoice for tenant {TenantId} via {Channel} to {Recipient}", 
                payload.TenantId, payload.Channel, payload.Recipient);
            throw;
        }
    }
}


