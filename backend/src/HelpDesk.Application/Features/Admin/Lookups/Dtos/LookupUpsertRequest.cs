namespace HelpDesk.Application.Features.Admin.Lookups.Dtos;

public class LookupUpsertRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
