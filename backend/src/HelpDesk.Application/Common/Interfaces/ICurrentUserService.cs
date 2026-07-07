namespace HelpDesk.Application.Common.Interfaces;

/// <summary>
/// Abstracts the identity of the caller executing the current request.
/// Implemented in the Api layer (backed by IHttpContextAccessor) once authentication is added.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? UserName { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Roles { get; }
}
