using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Services.Shared;

internal static class UserDisplayNameResolver
{
    internal static async Task<Dictionary<Guid, string>> GetNamesAsync(
        AppDbContext dbContext, IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken)
    {
        if (userIds.Count == 0)
        {
            return [];
        }

        return await dbContext.Users.AsNoTracking()
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, Name = u.FirstName + " " + u.LastName })
            .ToDictionaryAsync(u => u.Id, u => u.Name, cancellationToken);
    }

    internal static async Task<string> GetNameAsync(AppDbContext dbContext, Guid userId, CancellationToken cancellationToken)
    {
        var names = await GetNamesAsync(dbContext, [userId], cancellationToken);
        return names.GetValueOrDefault(userId, "Unknown");
    }
}
