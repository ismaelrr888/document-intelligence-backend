using System.Security.Cryptography;
using System.Text;
using Auth.Application.Abstractions.Security;
using Konscious.Security.Cryptography;

namespace Auth.Infrastructure.Security;

public sealed class PasswordHasher: IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            Iterations = 3,
            MemorySize = 65536
        };
        
        var hash = argon2.GetBytes(HashSize);

        return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
    }
    
    public bool Verify(string password, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var parts = passwordHash.Split(':');

        if (parts.Length != 2)
            return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expectedHash = Convert.FromBase64String(parts[1]);

        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 4,
            Iterations = 3,
            MemorySize = 65536
        };

        var actualHash = argon2.GetBytes(HashSize);

        return CryptographicOperations.FixedTimeEquals(
            actualHash,
            expectedHash);
    }
}