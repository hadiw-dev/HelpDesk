using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services.Shared;

/// <summary>
/// Ticket access rules shared by every service that reads or mutates a ticket
/// (<c>TicketService</c>, <c>AssignmentService</c>, <c>CommentService</c>): an Employee may only
/// act on tickets they created, Agent+ can act on any ticket.
/// </summary>
internal static class TicketAccessGuard
{
    internal static readonly string[] PrivilegedRoles = ["Admin", "Manager", "IT Support Agent"];
    internal static readonly string[] ManagerRoles = ["Admin", "Manager"];

    internal static bool IsAgentOrAbove(ICurrentUserService currentUser) => currentUser.Roles.Any(PrivilegedRoles.Contains);

    internal static bool IsManagerOrAdmin(ICurrentUserService currentUser) => currentUser.Roles.Any(ManagerRoles.Contains);

    internal static Guid RequireUserId(ICurrentUserService currentUser) =>
        currentUser.UserId ?? throw new UnauthorizedAppException("The current user could not be identified.");

    internal static async Task<Ticket> GetTicketForUserAsync(
        AppDbContext dbContext, ICurrentUserService currentUser, Guid ticketId, CancellationToken cancellationToken)
    {
        var ticket = await dbContext.Tickets
            .Include(t => t.Category)
            .Include(t => t.Priority)
            .Include(t => t.Status)
            .FirstOrDefaultAsync(t => t.Id == ticketId, cancellationToken);

        if (ticket is null)
        {
            throw new NotFoundAppException("Ticket not found.");
        }

        if (!IsAgentOrAbove(currentUser) && ticket.CreatedByUserId != RequireUserId(currentUser))
        {
            throw new ForbiddenAppException("You do not have access to this ticket.");
        }

        return ticket;
    }
}
