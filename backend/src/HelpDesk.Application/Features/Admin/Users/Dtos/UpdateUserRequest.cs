namespace HelpDesk.Application.Features.Admin.Users.Dtos;

public class UpdateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; }
}
