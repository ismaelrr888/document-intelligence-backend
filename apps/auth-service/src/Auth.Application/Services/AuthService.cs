using Auth.Application.Abstractions.Security;
using Auth.Application.Interfaces;
using Auth.Domain.User;

namespace Auth.Application.Services;

public class AuthService : IAuthService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository  _userRepository;

    public AuthService(IPasswordHasher passwordHasher, IUserRepository  userRepository)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        if (user is null)
        {
            return false;
        }

        return _passwordHasher.Verify(password, user.PasswordHash);
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