namespace HelpDesk.Application.Features.Admin.Users.Dtos;

public class AdminUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Department { get; set; }
    public string? JobTitle { get; set; }
    public bool IsActive { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public DateTime CreatedAt { get; set; }
}
