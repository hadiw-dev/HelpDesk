namespace HelpDesk.Application.Common.Exceptions;

/// <summary>
/// Base type for exceptions that map directly to an HTTP status code via the Api's exception middleware,
/// instead of always surfacing as a generic 500.
/// </summary>
public abstract class AppException : Exception
{
    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }

    public int StatusCode { get; }
}

public class NotFoundAppException : AppException
{
    public NotFoundAppException(string message) : base(message, StatusCodes.Status404NotFound)
    {
    }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message) : base(message, StatusCodes.Status401Unauthorized)
    {
    }
}

/// <summary>
/// The caller is authenticated but does not have permission to act on this specific resource
/// (e.g. an Employee accessing someone else's ticket). Distinct from <see cref="UnauthorizedAppException"/>,
/// which is for authentication failures (bad credentials, missing/invalid token).
/// </summary>
public class ForbiddenAppException : AppException
{
    public ForbiddenAppException(string message) : base(message, StatusCodes.Status403Forbidden)
    {
    }
}

public class ConflictAppException : AppException
{
    public ConflictAppException(string message) : base(message, StatusCodes.Status409Conflict)
    {
    }
}

public class ValidationAppException : AppException
{
    public ValidationAppException(string message) : base(message, StatusCodes.Status400BadRequest)
    {
    }
}

/// <summary>
/// Mirrors the small subset of <c>Microsoft.AspNetCore.Http.StatusCodes</c> this project needs,
/// so the Application layer doesn't take a dependency on ASP.NET Core.
/// </summary>
internal static class StatusCodes
{
    public const int Status400BadRequest = 400;
    public const int Status401Unauthorized = 401;
    public const int Status403Forbidden = 403;
    public const int Status404NotFound = 404;
    public const int Status409Conflict = 409;
}
