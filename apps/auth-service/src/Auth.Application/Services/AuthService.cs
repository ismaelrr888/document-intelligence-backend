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

    public AuthService(
        IPasswordHasher passwordHasher,
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
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

        var (token, expiresAtUtc) = _jwtTokenService.GenerateToken(user);

        return LoginResult.Success(token, expiresAtUtc);
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
}