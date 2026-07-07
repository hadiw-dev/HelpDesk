using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;
using HelpDesk.Domain.Identity;

namespace HelpDesk.Domain.Entities;

public class TicketAssignment : BaseEntity
{
    public Guid TicketId { get; set; }
    public Ticket Ticket { get; set; } = null!;

    public Guid? PreviousAssignedToUserId { get; set; }
    public ApplicationUser? PreviousAssignedToUser { get; set; }

    public Guid? AssignedToUserId { get; set; }
    public ApplicationUser? AssignedToUser { get; set; }

    public Guid? AssignedByUserId { get; set; }
    public ApplicationUser? AssignedByUser { get; set; }

    public AssignmentType AssignmentType { get; set; }
}
