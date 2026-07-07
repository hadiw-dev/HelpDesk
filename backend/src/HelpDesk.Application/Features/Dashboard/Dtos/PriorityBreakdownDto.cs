namespace HelpDesk.Application.Features.Dashboard.Dtos;

public class PriorityBreakdownDto
{
    public Guid PriorityId { get; set; }
    public string PriorityName { get; set; } = string.Empty;
    public int Count { get; set; }
}
