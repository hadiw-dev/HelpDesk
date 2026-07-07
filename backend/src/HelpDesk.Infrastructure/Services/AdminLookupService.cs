using HelpDesk.Application.Common.Exceptions;
using HelpDesk.Application.Features.Admin.Lookups.Dtos;
using HelpDesk.Application.Features.Admin.Lookups.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class AdminLookupService<TEntity> : IAdminLookupService<TEntity> where TEntity : LookupEntity, new()
{
    private readonly AppDbContext _dbContext;

    public AdminLookupService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<AdminLookupItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _dbContext.Set<TEntity>().AsNoTracking()
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(cancellationToken);

        return entities.Select(MapToDto).ToList();
    }

    public async Task<AdminLookupItemDto> CreateAsync(LookupUpsertRequest request, CancellationToken cancellationToken = default)
    {
        await EnsureNameIsUniqueAsync(request.Name, excludeId: null, cancellationToken);

        var entity = new TEntity
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive,
        };

        _dbContext.Set<TEntity>().Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task<AdminLookupItemDto> UpdateAsync(Guid id, LookupUpsertRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundAppException($"{typeof(TEntity).Name} not found.");

        await EnsureNameIsUniqueAsync(request.Name, excludeId: id, cancellationToken);

        entity.Name = request.Name;
        entity.Description = request.Description;
        entity.DisplayOrder = request.DisplayOrder;
        entity.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(entity);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new NotFoundAppException($"{typeof(TEntity).Name} not found.");

        _dbContext.Set<TEntity>().Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureNameIsUniqueAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        var isDuplicate = await _dbContext.Set<TEntity>()
            .AnyAsync(x => x.Name == name && x.Id != (excludeId ?? Guid.Empty), cancellationToken);

        if (isDuplicate)
        {
            throw new ConflictAppException($"A {typeof(TEntity).Name.ToLowerInvariant()} named '{name}' already exists.");
        }
    }

    private static AdminLookupItemDto MapToDto(TEntity entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Description = entity.Description,
        DisplayOrder = entity.DisplayOrder,
        IsActive = entity.IsActive,
    };
}
