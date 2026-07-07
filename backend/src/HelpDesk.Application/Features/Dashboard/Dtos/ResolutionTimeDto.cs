namespace HelpDesk.Application.Features.Dashboard.Dtos;

public class ResolutionTimeDto
{
    public double? OverallAverageResolutionHours { get; set; }
    public IReadOnlyList<PriorityResolutionTimeDto> ByPriority { get; set; } = [];
}

public class PriorityResolutionTimeDto
{
    public string PriorityName { get; set; } = string.Empty;
    public double? AverageResolutionHours { get; set; }
    public int TicketCount { get; set; }
}
