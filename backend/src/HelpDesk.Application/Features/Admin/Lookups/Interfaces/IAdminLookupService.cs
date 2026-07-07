using HelpDesk.Application.Features.Admin.Lookups.Dtos;
using HelpDesk.Domain.Entities;

namespace HelpDesk.Application.Features.Admin.Lookups.Interfaces;

/// <summary>
/// Generic CRUD for the three lookup tables (Category, Priority, Status) — they're identical in
/// shape (Name/Description/DisplayOrder/IsActive), so one generic service backs all three rather
/// than three near-duplicate implementations. Deleting soft-deletes (consistent with tickets);
/// existing tickets referencing a deleted lookup value keep their (now-hidden) reference intact.
/// </summary>
public interface IAdminLookupService<TEntity> where TEntity : LookupEntity
{
    Task<IReadOnlyList<AdminLookupItemDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<AdminLookupItemDto> CreateAsync(LookupUpsertRequest request, CancellationToken cancellationToken = default);

    Task<AdminLookupItemDto> UpdateAsync(Guid id, LookupUpsertRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
