using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Common.Utils;
using HelpDesk.Application.Features.Comments.Dtos;
using HelpDesk.Application.Features.Comments.Interfaces;
using HelpDesk.Application.Features.Notifications.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class CommentService : ICommentService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly INotificationService _notificationService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CommentService(
        AppDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        INotificationService notificationService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _notificationService = notificationService;
        _userManager = userManager;
    }

    public async Task<IReadOnlyList<CommentDto>> GetForTicketAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);

        var commentsQuery = _dbContext.TicketComments.AsNoTracking().Where(c => c.TicketId == ticketId);

        if (!TicketAccessGuard.IsAgentOrAbove(_currentUserService))
        {
            // Internal notes never reach an Employee, even on their own ticket.
            commentsQuery = commentsQuery.Where(c => !c.IsInternal);
        }

        var comments = await commentsQuery.OrderBy(c => c.CreatedAt).ToListAsync(cancellationToken);

        var names = await UserDisplayNameResolver.GetNamesAsync(
            _dbContext, comments.Select(c => c.UserId).Distinct().ToList(), cancellationToken);

        return comments.Select(c => MapToDto(c, names.GetValueOrDefault(c.UserId, "Unknown"))).ToList();
    }

    public async Task<CommentDto> AddAsync(Guid ticketId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        var ticket = await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);
        var currentUserId = TicketAccessGuard.RequireUserId(_currentUserService);

        if (request.IsInternal && !TicketAccessGuard.IsAgentOrAbove(_currentUserService))
        {
            throw new ForbiddenAppException("Only Agents and above can add internal notes.");
        }

        var comment = new TicketComment
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            UserId = currentUserId,
            Content = request.Content,
            IsInternal = request.IsInternal,
        };

        _dbContext.TicketComments.Add(comment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(
            currentUserId,
            request.IsInternal ? "InternalNoteAdded" : "CommentAdded",
            $"{(request.IsInternal ? "Internal note" : "Comment")} added to ticket {ticket.TicketNumber}.",
            null,
            cancellationToken);

        await NotifyStakeholdersAsync(ticket, comment, currentUserId, cancellationToken);

        var authorName = await UserDisplayNameResolver.GetNameAsync(_dbContext, currentUserId, cancellationToken);
        return MapToDto(comment, authorName);
    }

    private async Task NotifyStakeholdersAsync(Ticket ticket, TicketComment comment, Guid authorId, CancellationToken cancellationToken)
    {
        var stakeholderIds = new HashSet<Guid>();

        if (!comment.IsInternal)
        {
            stakeholderIds.Add(ticket.CreatedByUserId);
        }

        if (ticket.AssignedToUserId.HasValue)
        {
            stakeholderIds.Add(ticket.AssignedToUserId.Value);
        }

        stakeholderIds.Remove(authorId);

        foreach (var mentionedUserId in MentionParser.ExtractMentionedUserIds(comment.Content))
        {
            if (mentionedUserId == authorId)
            {
                continue;
            }

            // An internal note must never notify someone who isn't allowed to see it (e.g. the reporting Employee).
            if (comment.IsInternal && !await IsAgentOrAboveAsync(mentionedUserId))
            {
                continue;
            }

            await _notificationService.NotifyUserAsync(
                mentionedUserId,
                "You were mentioned",
                $"You were mentioned on ticket {ticket.TicketNumber} - {ticket.Title}.",
                NotificationType.Mention,
                ticket.Id,
                cancellationToken);

            stakeholderIds.Remove(mentionedUserId);
        }

        foreach (var stakeholderId in stakeholderIds)
        {
            await _notificationService.NotifyUserAsync(
                stakeholderId,
                comment.IsInternal ? "New internal note" : "New comment",
                $"{(comment.IsInternal ? "An internal note" : "A new comment")} was added to ticket {ticket.TicketNumber} - {ticket.Title}.",
                NotificationType.TicketCommented,
                ticket.Id,
                cancellationToken);
        }
    }

    private async Task<bool> IsAgentOrAboveAsync(Guid userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return false;
        }

        var roles = await _userManager.GetRolesAsync(user);
        return roles.Any(TicketAccessGuard.PrivilegedRoles.Contains);
    }

    private static CommentDto MapToDto(TicketComment comment, string authorName) => new()
    {
        Id = comment.Id,
        TicketId = comment.TicketId,
        Content = comment.Content,
        IsInternal = comment.IsInternal,
        AuthorId = comment.UserId,
        AuthorName = authorName,
        MentionedUserIds = MentionParser.ExtractMentionedUserIds(comment.Content),
        CreatedAt = comment.CreatedAt,
    };
}
