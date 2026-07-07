using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Common.Models;
using HelpDesk.Application.Features.Notifications.Dtos;
using HelpDesk.Application.Features.Notifications.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services.Shared;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailSender _emailSender;

    public NotificationService(AppDbContext dbContext, ICurrentUserService currentUserService, IEmailSender emailSender)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _emailSender = emailSender;
    }

    public async Task NotifyUserAsync(
        Guid userId,
        string title,
        string message,
        NotificationType type,
        Guid? relatedTicketId = null,
        CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            // e.g. a stale/garbage mention token pointing at a user that no longer exists — nothing to notify.
            return;
        }

        _dbContext.Notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Title = title,
            Message = message,
            Type = type,
            RelatedTicketId = relatedTicketId,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(user.Email))
        {
            await _emailSender.SendNotificationEmailAsync(user.Email, title, message, cancellationToken);
        }
    }

    public async Task<PagedResult<NotificationDto>> GetForCurrentUserAsync(
        int page, int pageSize, bool unreadOnly, CancellationToken cancellationToken = default)
    {
        var userId = TicketAccessGuard.RequireUserId(_currentUserService);

        var query = _dbContext.Notifications.AsNoTracking()
            .Include(n => n.RelatedTicket)
            .Where(n => n.UserId == userId);

        if (unreadOnly)
        {
            query = query.Where(n => !n.IsRead);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var boundedPage = Math.Max(page, 1);
        var boundedPageSize = Math.Clamp(pageSize, 1, 100);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .Skip((boundedPage - 1) * boundedPageSize)
            .Take(boundedPageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<NotificationDto>
        {
            Items = notifications.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = boundedPage,
            PageSize = boundedPageSize,
        };
    }

    public async Task<int> GetUnreadCountAsync(CancellationToken cancellationToken = default)
    {
        var userId = TicketAccessGuard.RequireUserId(_currentUserService);
        return await _dbContext.Notifications.AsNoTracking().CountAsync(n => n.UserId == userId && !n.IsRead, cancellationToken);
    }

    public async Task MarkAsReadAsync(Guid notificationId, CancellationToken cancellationToken = default)
    {
        var userId = TicketAccessGuard.RequireUserId(_currentUserService);

        var notification = await _dbContext.Notifications.FirstOrDefaultAsync(n => n.Id == notificationId, cancellationToken)
            ?? throw new NotFoundAppException("Notification not found.");

        if (notification.UserId != userId)
        {
            throw new ForbiddenAppException("You do not have access to this notification.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task MarkAllAsReadAsync(CancellationToken cancellationToken = default)
    {
        var userId = TicketAccessGuard.RequireUserId(_currentUserService);

        var unread = await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static NotificationDto MapToDto(Notification n) => new()
    {
        Id = n.Id,
        Title = n.Title,
        Message = n.Message,
        Type = n.Type.ToString(),
        IsRead = n.IsRead,
        ReadAt = n.ReadAt,
        RelatedTicketId = n.RelatedTicketId,
        RelatedTicketNumber = n.RelatedTicket?.TicketNumber,
        CreatedAt = n.CreatedAt,
    };
}
