using HelpDesk.Application.Features.Lookups.Dtos;

namespace HelpDesk.Application.Features.Lookups.Interfaces;

public interface ILookupService
{
    Task<IReadOnlyList<LookupItemDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetPrioritiesAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LookupItemDto>> GetStatusesAsync(CancellationToken cancellationToken = default);

    /// <summary>Users eligible to be assigned a ticket (Admin/Manager/IT Support Agent), for the Assignment Panel.</summary>
    Task<IReadOnlyList<LookupItemDto>> GetAssignableAgentsAsync(CancellationToken cancellationToken = default);
}
