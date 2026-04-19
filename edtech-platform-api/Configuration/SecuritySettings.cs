namespace edtech_platform_api.Configuration;

public sealed class SecuritySettings
{
    public const string SectionName = "Security";

    /// <summary>Validate antiforgery on payment POSTs (requires X-CSRF-TOKEN + GET /api/auth/csrf).</summary>
    public bool RequirePaymentCsrf { get; set; } = true;

    /// <summary>Return JWT in login JSON. Disable in production to reduce XSS blast radius (cookie is enough for browsers).</summary>
    public bool ExposeTokenInLoginResponse { get; set; }
}
