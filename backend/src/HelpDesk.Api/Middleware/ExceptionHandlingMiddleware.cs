using HelpDesk.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HelpDesk.Api.Middleware;

/// <summary>
/// Catches any exception that escapes the pipeline and converts it into an RFC 7807 ProblemDetails response,
/// so callers never see a raw stack trace or an unhandled 500 with no structured body. Known
/// <see cref="AppException"/> subtypes map to their specific status code; anything else is a 500.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, "Handled application exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Status = ex.StatusCode,
                Title = ex.Message,
                Instance = context.Request.Path,
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = ex.StatusCode;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Method} {Path}", context.Request.Method, context.Request.Path);

            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "An unexpected error occurred.",
                Detail = _environment.IsDevelopment() ? ex.ToString() : "Please contact support if the problem persists.",
                Instance = context.Request.Path,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1"
            };

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = problemDetails.Status.Value;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
