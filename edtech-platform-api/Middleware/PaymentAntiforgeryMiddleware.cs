using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;
using edtech_platform_api.Configuration;

namespace edtech_platform_api.Middleware;

/// <summary>
/// Validates antiforgery tokens for mutating /api/Payment requests when enabled.
/// Browsers must call GET /api/auth/csrf first and send X-CSRF-TOKEN on POST.
/// </summary>
public sealed class PaymentAntiforgeryMiddleware
{
    private readonly RequestDelegate _next;

    public PaymentAntiforgeryMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        IAntiforgery antiforgery,
        IOptions<SecuritySettings> securityOptions)
    {
        var settings = securityOptions.Value;
        if (!settings.RequirePaymentCsrf)
        {
            await _next(context);
            return;
        }

        var path = context.Request.Path;
        if (!path.StartsWithSegments("/api/Payment", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var method = context.Request.Method;
        if (method is not ("POST" or "PUT" or "PATCH" or "DELETE"))
        {
            await _next(context);
            return;
        }

        try
        {
            await antiforgery.ValidateRequestAsync(context);
        }
        catch (AntiforgeryValidationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Invalid or missing CSRF token. Call GET /api/auth/csrf with credentials, then send header X-CSRF-TOKEN on this request."
            });
            return;
        }

        await _next(context);
    }
}
