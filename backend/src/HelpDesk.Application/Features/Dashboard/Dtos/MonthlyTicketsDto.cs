namespace HelpDesk.Application.Features.Dashboard.Dtos;

public class MonthlyTicketsDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string MonthLabel { get; set; } = string.Empty;
    public int CreatedCount { get; set; }
    public int ResolvedCount { get; set; }
}
