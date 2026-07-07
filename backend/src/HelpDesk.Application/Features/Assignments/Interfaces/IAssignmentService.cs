using HelpDesk.Application.Features.Assignments.Dtos;
using HelpDesk.Application.Features.Tickets.Dtos;

namespace HelpDesk.Application.Features.Assignments.Interfaces;

public interface IAssignmentService
{
    /// <summary>Manually assigns or reassigns a ticket. Pass a null <c>AssignedToUserId</c> to unassign.</summary>
    Task<TicketDto> AssignAsync(Guid ticketId, AssignTicketRequest request, CancellationToken cancellationToken = default);

    /// <summary>Assigns the ticket to the next eligible IT Support Agent in round-robin order.</summary>
    Task<TicketDto> AutoAssignAsync(Guid ticketId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AssignmentHistoryEntryDto>> GetHistoryAsync(Guid ticketId, CancellationToken cancellationToken = default);
}
