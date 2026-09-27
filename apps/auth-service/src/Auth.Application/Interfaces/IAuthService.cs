using Auth.Application.Models;

namespace Auth.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string email, string password);

    /// <summary>
    /// Exchanges a valid, unexpired refresh token for a new access/refresh token
    /// pair, rotating (revoking) the presented refresh token in the process.
    /// </summary>
    Task<LoginResult> RefreshAsync(string refreshToken);

    /// <summary>
    /// Revokes a refresh token so it can no longer be used to obtain new access
    /// tokens. Idempotent: returns true whether or not the token was active.
    /// </summary>
    Task<bool> LogoutAsync(string refreshToken);
    
    Task LogoutAllSessionsAsync(Guid userId);
}