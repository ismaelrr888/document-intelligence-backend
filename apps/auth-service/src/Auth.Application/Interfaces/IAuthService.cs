namespace Auth.Application.Interfaces;

public interface IAuthService
{
    Task<bool> LoginAsync(string email, string password);
    Task<bool> RegisterAsync(string email, string password);
}