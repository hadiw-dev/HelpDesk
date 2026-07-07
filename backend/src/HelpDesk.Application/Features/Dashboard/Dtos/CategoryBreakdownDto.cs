namespace HelpDesk.Application.Features.Dashboard.Dtos;

public class CategoryBreakdownDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}
