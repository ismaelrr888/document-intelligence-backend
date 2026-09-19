using Auth.Application.Abstractions.Security;
using Auth.Application.Interfaces;

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
}