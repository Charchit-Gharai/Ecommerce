
namespace ECommerce.Api.Middleware;

public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;

        // Prevent MIME-type sniffing.
        headers["X-Content-Type-Options"] = "nosniff";

        // Prevent the API from being embedded in frames.
        headers["X-Frame-Options"] = "DENY";

        // Limit referrer information.
        headers["Referrer-Policy"] =
            "strict-origin-when-cross-origin";

        // Disable unnecessary browser capabilities.
        headers["Permissions-Policy"] =
            "camera=(), microphone=(), geolocation=()";

        // Apply a restrictive policy to API responses
        if (!context.Request.Path.StartsWithSegments("/swagger"))
        {
            headers["Content-Security-Policy"] =
                "default-src 'none'; " +
                "base-uri 'none'; " +
                "form-action 'none'; " +
                "frame-ancestors 'none'";
        }

        await _next(context);
    }
}
