namespace HelpDesk.Application.Features.Assignments.Dtos;

public class AssignmentHistoryEntryDto
{
    public Guid Id { get; set; }
    public string? PreviousAssignedToName { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedByName { get; set; }
    public string AssignmentType { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
}
