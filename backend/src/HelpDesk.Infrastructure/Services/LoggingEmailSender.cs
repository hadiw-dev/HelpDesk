using HelpDesk.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace HelpDesk.Infrastructure.Services;

/// <summary>
/// Logs the password reset token instead of sending a real email. There is no SMTP/email provider
/// integration yet; this keeps the forgot/reset-password flow fully testable until one is added.
/// </summary>
public class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Password reset requested for {Email}. Reset token: {ResetToken}",
            toEmail,
            resetToken);

        return Task.CompletedTask;
    }

    public Task SendNotificationEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Notification email to {Email}: {Subject} - {Body}",
            toEmail,
            subject,
            body);

        return Task.CompletedTask;
    }
}
