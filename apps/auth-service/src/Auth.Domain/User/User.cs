namespace Auth.Domain.User;

public sealed class User
{
    public Guid Id { get; private set; }

    public string Email { get; private set; }

    public string PasswordHash { get; private set; }

    public bool IsEmailConfirmed { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    private User()
    {
         //Constructor required later for EF Core
    }
    
    public User(string email, string passwordHash)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException(
                "Email is required.",
                nameof(email)
            );
        }

        if (string.IsNullOrEmpty(passwordHash))
        {
            throw new ArgumentException(
                "Password hash is required.",
                nameof(passwordHash)
            );
        }

        Id = Guid.NewGuid();
        Email = email;
        PasswordHash = passwordHash;
        IsEmailConfirmed = false;
        CreatedAt = DateTimeOffset.UtcNow;
    }
    
    public void ConfirmEmail()
    {
        IsEmailConfirmed = true;
    }
}