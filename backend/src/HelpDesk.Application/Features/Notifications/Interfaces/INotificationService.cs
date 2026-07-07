using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Notifications.Dtos;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Features.Notifications.Interfaces;

/// <summary>
/// Dispatches in-app notifications (persisted + logged via the email stub) and serves the
/// signed-in user's own notification feed for the Notification Center.
/// </summary>
public interface INotificationService
{
    Task NotifyUserAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedTicketId = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<NotificationDto>> GetForCurrentUserAsync(
        int page, int pageSize, bool unreadOnly, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default);

    Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default);

    Task MarkAllAsReadAsync(CancellationToken cancellationToken = default);
}
