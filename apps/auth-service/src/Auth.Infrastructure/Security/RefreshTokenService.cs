using System.Security.Cryptography;
using System.Text;
using Auth.Application.Abstractions.Security;
using Microsoft.Extensions.Options;

namespace Auth.Infrastructure.Security;

/// <summary>
/// Issues opaque, high-entropy refresh tokens. Only the SHA-256 hash of the raw
/// token is ever persisted, so a database leak alone cannot be used to forge
/// or replay a session.
/// </summary>
public sealed class RefreshTokenService : IRefreshTokenService
{
    private const int TokenSizeInBytes = 64;

    private readonly JwtOptions _options;

    public RefreshTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public (string RawToken, string TokenHash, DateTimeOffset ExpiresAtUtc) GenerateToken()
    {
        var rawToken = GenerateRawToken();
        var expiresAtUtc = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenExpirationDays);

        return (rawToken, Hash(rawToken), expiresAtUtc);
    }

    public string Hash(string rawToken)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(hashBytes);
    }

    private static string GenerateRawToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);

        // URL-safe base64 so the token can be transported/stored without escaping.
        return Convert.ToBase64String(randomBytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
