using Serilog;
using Microsoft.Extensions.Options;
using OrderManagement.Application.Abstractions.Notifications;
using OrderManagement.Infrastructure.Options;

namespace OrderManagement.Infrastructure.Messaging;

public sealed class EmailNotificationService(
    Serilog.ILogger logger,
    IOptions<EmailOptions> options) : IEmailNotificationService
{
    private readonly EmailOptions _options = options.Value;

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.Information("Sending email to {Email} via {Provider} with subject {Subject}", toEmail, _options.Provider, subject);
            // TODO: integrate with SMTP/Email provider
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error sending email to {Email} with subject {Subject}", toEmail, subject);
            throw;
        }
    }
}


