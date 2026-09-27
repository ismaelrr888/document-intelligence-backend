using Auth.Application.Models;

namespace Auth.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string email, string password);
}