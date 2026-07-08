namespace HelpDesk.Api.Middleware;

/// <summary>
/// Adds baseline security response headers. This is a JSON API with no server-rendered HTML, so the
/// headers that matter are the ones that stop a browser from doing something unexpected with a JSON
/// response — not a full HTML-page CSP, which wouldn't apply here.
/// </summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            // Stops a browser from trying to sniff/execute a JSON response body as HTML/script.
            headers["X-Content-Type-Options"] = "nosniff";

            // No legitimate reason for this API's responses to render inside a frame.
            headers["X-Frame-Options"] = "DENY";

            // Don't leak the full request URL (which can carry query-string data) to third-party referrers.
            headers["Referrer-Policy"] = "no-referrer";

            return Task.CompletedTask;
        });

        return _next(context);
    }
}
