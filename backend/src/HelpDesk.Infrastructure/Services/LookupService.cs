using HelpDesk.Application.Features.Lookups.Dtos;
using HelpDesk.Application.Features.Lookups.Interfaces;
using HelpDesk.Domain.Identity;
using HelpDesk.Infrastructure.Persistence;
using HelpDesk.Infrastructure.Services.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services;

public class LookupService : ILookupService
{
    private readonly AppDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;

    public LookupService(AppDbContext dbContext, UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _userManager = userManager;
    }

    public Task<IReadOnlyList<LookupItemDto>> GetCategoriesAsync(CancellationToken cancellationToken = default) =>
        GetOrderedAsync(_dbContext.Categories.AsNoTracking(), cancellationToken);

    public Task<IReadOnlyList<LookupItemDto>> GetPrioritiesAsync(CancellationToken cancellationToken = default) =>
        GetOrderedAsync(_dbContext.Priorities.AsNoTracking(), cancellationToken);

    public Task<IReadOnlyList<LookupItemDto>> GetStatusesAsync(CancellationToken cancellationToken = default) =>
        GetOrderedAsync(_dbContext.Statuses.AsNoTracking(), cancellationToken);

    public async Task<IReadOnlyList<LookupItemDto>> GetAssignableAgentsAsync(CancellationToken cancellationToken = default)
    {
        var agents = new Dictionary<Guid, ApplicationUser>();

        foreach (var role in TicketAccessGuard.PrivilegedRoles)
        {
            foreach (var user in await _userManager.GetUsersInRoleAsync(role))
            {
                agents.TryAdd(user.Id, user);
            }
        }

        return agents.Values
            .Where(u => u.IsActive)
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Select(u => new LookupItemDto { Id = u.Id, Name = $"{u.FirstName} {u.LastName}" })
            .ToList();
    }

    private static async Task<IReadOnlyList<LookupItemDto>> GetOrderedAsync(
        IQueryable<Domain.Entities.LookupEntity> query, CancellationToken cancellationToken)
    {
        return await query
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .Select(x => new LookupItemDto { Id = x.Id, Name = x.Name })
            .ToListAsync(cancellationToken);
    }
}
