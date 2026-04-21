namespace edtech_platform_api.Configuration;

public sealed class PasswordResetSettings
{
    public const string SectionName = "PasswordReset";

    public string FrontendBaseUrl { get; set; } = string.Empty;
}
