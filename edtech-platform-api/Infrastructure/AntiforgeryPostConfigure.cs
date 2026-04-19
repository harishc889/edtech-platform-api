using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using edtech_platform_api.Configuration;

namespace edtech_platform_api.Infrastructure;

public sealed class AntiforgeryPostConfigure : IPostConfigureOptions<AntiforgeryOptions>
{
    private readonly IOptions<CookieAuthSettings> _cookieAuth;

    public AntiforgeryPostConfigure(IOptions<CookieAuthSettings> cookieAuth)
    {
        _cookieAuth = cookieAuth;
    }

    public void PostConfigure(string? name, AntiforgeryOptions options)
    {
        var cookieAuth = _cookieAuth.Value;
        options.HeaderName = "X-CSRF-TOKEN";
        options.Cookie.Name = "csrf";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = CookieAuthHelper.ParseSameSite(cookieAuth.SameSite);
        options.Cookie.SecurePolicy = cookieAuth.Secure ? CookieSecurePolicy.Always : CookieSecurePolicy.SameAsRequest;
        if (!string.IsNullOrWhiteSpace(cookieAuth.Domain))
            options.Cookie.Domain = cookieAuth.Domain;
        options.Cookie.Path = string.IsNullOrWhiteSpace(cookieAuth.Path) ? "/" : cookieAuth.Path;
        options.SuppressXFrameOptionsHeader = true;
    }
}
