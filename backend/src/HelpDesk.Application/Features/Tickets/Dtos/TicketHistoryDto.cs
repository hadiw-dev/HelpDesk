namespace HelpDesk.Application.Features.Tickets.Dtos;

public class TicketHistoryDto
{
    public Guid Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string? ChangedByName { get; set; }
    public DateTime ChangedAt { get; set; }
}
