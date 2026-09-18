namespace Auth.Application.Abstractions.Security;

public interface IPasswordHasher
{
    string Hash(string  password);
    
    bool Verify(string hash, string passwordHash);
}