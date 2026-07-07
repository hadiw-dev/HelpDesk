namespace HelpDesk.Application.Common.Interfaces;

public interface IActivityLogService
{
    Task LogAsync(Guid? userId, string action, string? details, string? ipAddress, CancellationToken cancellationToken = default);
}
