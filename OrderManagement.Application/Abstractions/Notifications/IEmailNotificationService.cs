namespace OrderManagement.Application.Abstractions.Notifications;

public interface IEmailNotificationService
{
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}


