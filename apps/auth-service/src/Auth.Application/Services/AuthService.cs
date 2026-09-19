using Auth.Application.Abstractions.Security;
using Auth.Application.Interfaces;

namespace Auth.Application.Services;

public class AuthService
{
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository  _userRepository;

    public AuthService(IPasswordHasher passwordHasher, IUserRepository  userRepository)
    {
        _passwordHasher = passwordHasher;
        _userRepository = userRepository;
    }
}