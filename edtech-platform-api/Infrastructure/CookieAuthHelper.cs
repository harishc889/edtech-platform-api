using Microsoft.AspNetCore.Http;
using edtech_platform_api.Configuration;

namespace edtech_platform_api.Infrastructure;

public static class CookieAuthHelper
{
    public static CookieOptions CreateAuthCookieOptions(CookieAuthSettings settings, DateTimeOffset? expires = null)
    {
        var sameSite = ParseSameSite(settings.SameSite);
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = settings.Secure,
            SameSite = sameSite,
            Path = string.IsNullOrEmpty(settings.Path) ? "/" : settings.Path
        };

        if (!string.IsNullOrWhiteSpace(settings.Domain))
            options.Domain = settings.Domain;

        if (expires.HasValue)
            options.Expires = expires;

        return options;
    }

    public static SameSiteMode ParseSameSite(string? value) =>
        value?.Trim().ToLowerInvariant() switch
        {
            "none" => SameSiteMode.None,
            "lax" => SameSiteMode.Lax,
            "strict" => SameSiteMode.Strict,
            _ => SameSiteMode.Lax
        };
}
