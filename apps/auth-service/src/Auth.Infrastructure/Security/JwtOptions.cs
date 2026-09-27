namespace Auth.Infrastructure.Security;

/// <summary>
/// Strongly typed JWT settings bound from configuration (section "Jwt").
/// The signing secret must never be hardcoded; provide it via appsettings,
/// user-secrets or environment variables depending on the environment.
/// </summary>
public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 60;

    public int RefreshTokenExpirationDays { get; set; } = 7;
}
