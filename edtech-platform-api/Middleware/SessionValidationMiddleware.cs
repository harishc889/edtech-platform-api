using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using edtech_platform_api.Data;

namespace edtech_platform_api.Middleware
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;

            // If request is not authenticated, skip session validation
            if (user?.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            // Extract sessionId claim
            var sessionClaim = user.FindFirst("sessionId");
            if (sessionClaim == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Session claim missing" });
                return;
            }

            var sessionId = sessionClaim.Value;

            // Resolve DbContext from request services
            var db = context.RequestServices.GetService(typeof(AppDbContext)) as AppDbContext;
            if (db == null)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsJsonAsync(new { error = "Database unavailable" });
                return;
            }

            // Check if session is active
            var isActive = await db.UserSessions.AnyAsync(s => s.SessionId == sessionId && s.IsActive);
            if (!isActive)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { error = "Session is not active" });
                return;
            }

            await _next(context);
        }
    }
}
