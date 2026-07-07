using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Application.Features.Assignments.Dtos;
using HelpDesk.Application.Features.Assignments.Interfaces;
using HelpDesk.Application.Features.Notifications.Interfaces;
using HelpDesk.Application.Features.Tickets.Dtos;
using HelpDesk.Application.Features.Tickets.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class AssignmentService : IAssignmentService
{
    private readonly AppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IActivityLogService _activityLogService;
    private readonly INotificationService _notificationService;
    private readonly ITicketService _ticketService;
    private readonly UserManager<ApplicationUser> _userManager;

    public AssignmentService(
        AppDbContext dbContext,
        ICurrentUserService currentUserService,
        IActivityLogService activityLogService,
        INotificationService notificationService,
        ITicketService ticketService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _activityLogService = activityLogService;
        _notificationService = notificationService;
        _ticketService = ticketService;
        _userManager = userManager;
    }

    public async Task<TicketDto> AssignAsync(Guid ticketId, AssignTicketRequest request, CancellationToken cancellationToken = default)
    {
        RequireAgentOrAbove();

        var ticket = await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);

        if (request.AssignedToUserId.HasValue)
        {
            await EnsureAssigneeIsEligibleAsync(request.AssignedToUserId.Value, cancellationToken);
        }

        await ApplyAssignmentAsync(ticket, request.AssignedToUserId, AssignmentType.Manual, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(
            TicketAccessGuard.RequireUserId(_currentUserService),
            "TicketAssigned",
            $"Ticket {ticket.TicketNumber} assigned.",
            null,
            cancellationToken);

        return await _ticketService.GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<TicketDto> AutoAssignAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        RequireAgentOrAbove();

        var ticket = await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);

        var agentIds = await GetEligibleRoundRobinAgentIdsAsync();
        if (agentIds.Count == 0)
        {
            throw new ValidationAppException("No IT Support Agents are available for auto-assignment.");
        }

        var lastRoundRobinAssignment = await _dbContext.TicketAssignments.AsNoTracking()
            .Where(a => a.AssignmentType == AssignmentType.RoundRobin)
            .OrderByDescending(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var nextAgentId = PickNextAgent(agentIds, lastRoundRobinAssignment?.AssignedToUserId);

        await ApplyAssignmentAsync(ticket, nextAgentId, AssignmentType.RoundRobin, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _activityLogService.LogAsync(
            TicketAccessGuard.RequireUserId(_currentUserService),
            "TicketAutoAssigned",
            $"Ticket {ticket.TicketNumber} auto-assigned via round robin.",
            null,
            cancellationToken);

        return await _ticketService.GetByIdAsync(ticket.Id, cancellationToken);
    }

    public async Task<IReadOnlyList<AssignmentHistoryEntryDto>> GetHistoryAsync(Guid ticketId, CancellationToken cancellationToken = default)
    {
        // Ensures access control (throws if the ticket doesn't exist or the caller can't see it).
        await TicketAccessGuard.GetTicketForUserAsync(_dbContext, _currentUserService, ticketId, cancellationToken);

        var entries = await _dbContext.TicketAssignments.AsNoTracking()
            .Where(a => a.TicketId == ticketId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);

        var userIds = entries
            .SelectMany(a => new[] { a.PreviousAssignedToUserId, a.AssignedToUserId, a.AssignedByUserId })
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var names = await UserDisplayNameResolver.GetNamesAsync(_dbContext, userIds, cancellationToken);

        return entries.Select(a => new AssignmentHistoryEntryDto
        {
            Id = a.Id,
            PreviousAssignedToName = a.PreviousAssignedToUserId.HasValue ? names.GetValueOrDefault(a.PreviousAssignedToUserId.Value, "Unknown") : "Unassigned",
            AssignedToName = a.AssignedToUserId.HasValue ? names.GetValueOrDefault(a.AssignedToUserId.Value, "Unknown") : "Unassigned",
            AssignedByName = a.AssignedByUserId.HasValue ? names.GetValueOrDefault(a.AssignedByUserId.Value, "Unknown") : "System (Round Robin)",
            AssignmentType = a.AssignmentType.ToString(),
            AssignedAt = a.CreatedAt,
        }).ToList();
    }

    private async Task ApplyAssignmentAsync(Ticket ticket, Guid? newAssigneeId, AssignmentType assignmentType, CancellationToken cancellationToken)
    {
        var previousAssigneeId = ticket.AssignedToUserId;
        if (previousAssigneeId == newAssigneeId)
        {
            return;
        }

        var namedUserIds = new[] { previousAssigneeId, newAssigneeId }.Where(id => id.HasValue).Select(id => id!.Value).ToList();
        var names = await UserDisplayNameResolver.GetNamesAsync(_dbContext, namedUserIds, cancellationToken);

        var oldName = previousAssigneeId.HasValue ? names.GetValueOrDefault(previousAssigneeId.Value, "Unknown") : "Unassigned";
        var newName = newAssigneeId.HasValue ? names.GetValueOrDefault(newAssigneeId.Value, "Unknown") : "Unassigned";

        TicketHistoryRecorder.Record(_dbContext, _currentUserService, ticket.Id, "AssignedTo", oldName, newName);

        _dbContext.TicketAssignments.Add(new TicketAssignment
        {
            Id = Guid.NewGuid(),
            TicketId = ticket.Id,
            PreviousAssignedToUserId = previousAssigneeId,
            AssignedToUserId = newAssigneeId,
            AssignedByUserId = assignmentType == AssignmentType.Manual ? _currentUserService.UserId : null,
            AssignmentType = assignmentType,
        });

        ticket.AssignedToUserId = newAssigneeId;

        if (newAssigneeId.HasValue)
        {
            await _notificationService.NotifyUserAsync(
                newAssigneeId.Value,
                "Ticket assigned to you",
                $"Ticket {ticket.TicketNumber} - {ticket.Title} has been assigned to you.",
                NotificationType.TicketAssigned,
                ticket.Id,
                cancellationToken);
        }
    }

    private async Task EnsureAssigneeIsEligibleAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = await _userManager.FindByIdAsync(userId.ToString())
            ?? throw new ValidationAppException("Assigned user not found.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Any(TicketAccessGuard.PrivilegedRoles.Contains))
        {
            throw new ValidationAppException("Tickets can only be assigned to an IT Support Agent, Manager, or Admin.");
        }
    }

    private async Task<List<Guid>> GetEligibleRoundRobinAgentIdsAsync()
    {
        var agents = await _userManager.GetUsersInRoleAsync("IT Support Agent");
        return agents
            .Where(a => a.IsActive)
            .OrderBy(a => a.Id)
            .Select(a => a.Id)
            .ToList();
    }

    private static Guid PickNextAgent(List<Guid> agentIds, Guid? lastAssignedAgentId)
    {
        if (!lastAssignedAgentId.HasValue)
        {
            return agentIds[0];
        }

        var lastIndex = agentIds.IndexOf(lastAssignedAgentId.Value);
        if (lastIndex < 0)
        {
            // The previously round-robin-assigned agent is no longer eligible (e.g. role changed) — restart the rotation.
            return agentIds[0];
        }

        return agentIds[(lastIndex + 1) % agentIds.Count];
    }

    private void RequireAgentOrAbove()
    {
        if (!TicketAccessGuard.IsAgentOrAbove(_currentUserService))
        {
            throw new ForbiddenAppException("You do not have permission to assign tickets.");
        }
    }
}
