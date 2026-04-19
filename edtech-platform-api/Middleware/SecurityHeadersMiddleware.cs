using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace edtech_platform_api.Middleware;

/// <summary>
/// Defense-in-depth response headers for browsers. Does not replace auth, CSRF, or CORS configuration.
/// </summary>
public sealed class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _env;

    public SecurityHeadersMiddleware(RequestDelegate next, IHostEnvironment env)
    {
        _next = next;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.OnStarting(() =>
        {
            var headers = context.Response.Headers;

            if (!headers.ContainsKey("X-Content-Type-Options"))
                headers["X-Content-Type-Options"] = "nosniff";

            if (!headers.ContainsKey("X-Frame-Options"))
                headers["X-Frame-Options"] = "DENY";

            if (!headers.ContainsKey("Referrer-Policy"))
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";

            if (!headers.ContainsKey("Permissions-Policy"))
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";

            if (!_env.IsDevelopment() && !headers.ContainsKey("Strict-Transport-Security"))
                headers["Strict-Transport-Security"] = "max-age=31536000; includeSubDomains";

            return Task.CompletedTask;
        });

        await _next(context);
    }
}
