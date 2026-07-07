namespace HelpDesk.Application.Common.Interfaces;

public interface IEmailSender
{
    Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default);

    /// <summary>Email-notification stub for in-app notification events (assignment, comments, mentions, ...).</summary>
    Task SendNotificationEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}
