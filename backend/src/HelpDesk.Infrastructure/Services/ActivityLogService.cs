using HelpDesk.Application.Common.Interfaces;
using HelpDesk.Domain.Entities;
using HelpDesk.Infrastructure.Persistence;

namespace HelpDesk.Infrastructure.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly AppDbContext _dbContext;

    public ActivityLogService(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogAsync(Guid? userId, string action, string? details, string? ipAddress, CancellationToken cancellationToken = default)
    {
        _dbContext.ActivityLogs.Add(new ActivityLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Action = action,
            EntityName = "User",
            EntityId = userId,
            Details = details,
            IpAddress = ipAddress,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
