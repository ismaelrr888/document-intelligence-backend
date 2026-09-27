namespace Auth.Application.Abstractions.Security;

/// <summary>
/// Abstraction for issuing and hashing refresh tokens. The raw token is only ever
/// handed to the client; the application/infrastructure layers only ever store
/// and compare its hash.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Generates a new opaque refresh token.
    /// </summary>
    /// <returns>The raw token (to return to the client), its hash (to persist),
    /// and its UTC expiration timestamp.</returns>
    (string RawToken, string TokenHash, DateTimeOffset ExpiresAtUtc) GenerateToken();

    /// <summary>
    /// Hashes a raw refresh token so it can be looked up/compared against
    /// persisted hashes without ever storing the raw value.
    /// </summary>
    string Hash(string rawToken);
}
