namespace HelpDesk.Domain.Enums;

public enum NotificationType
{
    TicketCreated = 0,
    TicketAssigned = 1,
    TicketUpdated = 2,
    TicketCommented = 3,
    TicketResolved = 4,
    TicketClosed = 5,
    System = 6,
    Mention = 7
}
