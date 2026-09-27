namespace Auth.Domain.User;

/// <summary>
/// A rotating, single-use credential that lets a client obtain a new access token
/// without re-authenticating. Only the hash is ever persisted; the raw value is
/// returned to the client once and never stored.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; }

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public bool IsActive => RevokedAtUtc is null && DateTimeOffset.UtcNow < ExpiresAtUtc;

    private RefreshToken()
    {
        //Constructor required later for EF Core
    }

    public RefreshToken(Guid userId, string tokenHash, DateTimeOffset expiresAtUtc)
    {
        if (string.IsNullOrEmpty(tokenHash))
        {
            throw new ArgumentException(
                "Token hash is required.",
                nameof(tokenHash));
        }

        Id = Guid.NewGuid();
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Marks the token as no longer usable, either because it was consumed
    /// (rotation on refresh) or explicitly revoked (logout).
    /// </summary>
    public void Revoke()
    {
        RevokedAtUtc = DateTimeOffset.UtcNow;
    }
}
