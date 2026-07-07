using HelpDesk.Domain.Common;

namespace HelpDesk.Domain.Entities;

public abstract class LookupEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
