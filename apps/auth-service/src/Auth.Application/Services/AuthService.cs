using Auth.Application.Abstractions.Security;
using Auth.Application.Interfaces;
using Auth.Application.Models;
using Auth.Domain.User;

namespace Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository  _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AuthService(
        IPasswordHasher passwordHasher,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IRefreshTokenService refreshTokenService,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _refreshTokenService = refreshTokenService;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        // Same generic failure for "user not found" and "wrong password" to avoid
        // leaking which case occurred (prevents user enumeration).
        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            return LoginResult.Fail();
        }

        return await IssueTokensAsync(user);
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        var existingUser = await _userRepository.GetByEmailAsync(email);

        if (existingUser is not null)
        {
            return false;
        }

        var passwordHash = _passwordHasher.Hash(password);

        var user = new User(email, passwordHash);
        
        await _userRepository.AddAsync(user);

        return true;
    }

    public async Task<LoginResult> RefreshAsync(string refreshToken)
    {
        var tokenHash = _refreshTokenService.Hash(refreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (existingToken is null || !existingToken.IsActive)
        {
            return LoginResult.Fail();
        }

        var user = await _userRepository.GetByIdAsync(existingToken.UserId);

        if (user is null)
        {
            return LoginResult.Fail();
        }

        // Rotate: the presented token is single-use. Revoking it here means a
        // stolen/replayed refresh token stops working after its first use.
        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken);

        return await IssueTokensAsync(user);
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        var tokenHash = _refreshTokenService.Hash(refreshToken);
        var existingToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash);

        if (existingToken is null || !existingToken.IsActive)
        {
            return false;
        }

        existingToken.Revoke();
        await _refreshTokenRepository.UpdateAsync(existingToken);

        return true;
    }

    private async Task<LoginResult> IssueTokensAsync(User user)
    {
        var (accessToken, expiresAtUtc) = _jwtTokenService.GenerateToken(user);
        var (rawRefreshToken, refreshTokenHash, refreshExpiresAtUtc) = _refreshTokenService.GenerateToken();

        var refreshToken = new RefreshToken(user.Id, refreshTokenHash, refreshExpiresAtUtc);
        await _refreshTokenRepository.AddAsync(refreshToken);

        return LoginResult.Success(accessToken, expiresAtUtc, rawRefreshToken);
    }
    
    public async Task LogoutAllSessionsAsync(Guid userId)
    {
        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
    }
}
