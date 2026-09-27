namespace Auth.Application.Models;

/// <summary>
/// Result of an authentication attempt. On success, contains the issued JWT.
/// On failure, no details are exposed about the reason (unknown user vs wrong password)
/// to avoid user enumeration.
/// </summary>
public sealed record LoginResult(
    bool Succeeded,
    string? Token,
    DateTimeOffset? ExpiresAtUtc,
    string? RefreshToken)
{
    public static LoginResult Fail() => new(false, null, null, null);

    public static LoginResult Success(string token, DateTimeOffset expiresAtUtc, string refreshToken) =>
        new(true, token, expiresAtUtc, refreshToken);
}
