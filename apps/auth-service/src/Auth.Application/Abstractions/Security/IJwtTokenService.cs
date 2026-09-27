using Auth.Domain.User;

namespace Auth.Application.Abstractions.Security;

/// <summary>
/// Abstraction for issuing JWT access tokens. The concrete implementation
/// (signing algorithm, configuration source, etc.) lives in Infrastructure.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a signed JWT for the given user.
    /// </summary>
    /// <returns>The encoded token and its UTC expiration timestamp.</returns>
    (string Token, DateTimeOffset ExpiresAtUtc) GenerateToken(User user);
}
