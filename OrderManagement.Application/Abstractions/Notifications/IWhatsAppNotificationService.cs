namespace OrderManagement.Application.Abstractions.Notifications;

public interface IWhatsAppNotificationService
{
    Task SendTemplateAsync(string phoneNumber, string templateName, object payload, CancellationToken cancellationToken = default);
}


