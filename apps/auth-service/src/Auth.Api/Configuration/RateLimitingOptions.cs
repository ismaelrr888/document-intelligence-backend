namespace Auth.Api.Configuration;

/// <summary>
/// Strongly typed rate-limiting settings bound from configuration (section "RateLimiting").
/// </summary>
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Name of the ASP.NET Core rate limiter policy applied to the login endpoint.
    /// </summary>
    public const string LoginPolicyName = "auth-login";

    /// <summary>
    /// Max number of login attempts allowed per client (IP) within <see cref="LoginWindowSeconds"/>.
    /// </summary>
    public int LoginPermitLimit { get; set; } = 5;

    /// <summary>
    /// Size, in seconds, of the fixed window used to count login attempts.
    /// </summary>
    public int LoginWindowSeconds { get; set; } = 60;
}
