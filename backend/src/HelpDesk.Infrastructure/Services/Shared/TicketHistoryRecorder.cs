using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;

namespace HelpDesk.Infrastructure.Services.Shared;

internal static class TicketHistoryRecorder
{
    /// <summary>No-ops when <paramref name="oldValue"/> equals <paramref name="newValue"/> so a no-change
    /// update doesn't produce a "changed from X to X" row.</summary>
    internal static void Record(
        AppDbContext dbContext, ICurrentUserService currentUser, Guid ticketId, string fieldName, string? oldValue, string? newValue)
    {
        if (oldValue == newValue)
        {
            return;
        }

        dbContext.TicketHistories.Add(new TicketHistory
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            ChangedByUserId = currentUser.UserId,
            FieldName = fieldName,
            OldValue = oldValue,
            NewValue = newValue,
        });
    }
}
