namespace edtech_platform_api.Configuration;

public sealed class CookieAuthSettings
{
    public const string SectionName = "CookieAuth";

    /// <summary>When false, cookies work on http://localhost (development only).</summary>
    public bool Secure { get; set; } = true;

    /// <summary>Strict | Lax | None. Cross-site SPA + API needs None with Secure and trusted CORS.</summary>
    public string SameSite { get; set; } = "Lax";

    /// <summary>Optional e.g. .yourdomain.com for subdomains. Omit for host-only cookie.</summary>
    public string? Domain { get; set; }

    public string Path { get; set; } = "/";
}
