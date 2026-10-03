using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Validation;

/// <summary>
/// Enforces a minimum password complexity for new credentials: at least one
/// lowercase letter, one uppercase letter, one digit and one special
/// character. Only intended for registration/password-change flows — login
/// must keep accepting whatever password a user already registered with.
/// </summary>
public sealed class StrongPasswordAttribute : ValidationAttribute
{
    public StrongPasswordAttribute()
    {
        ErrorMessage = "Password must contain at least one uppercase letter, " +
                       "one lowercase letter, one digit and one special character.";
    }

    public override bool IsValid(object? value)
    {
        if (value is not string password || password.Length == 0)
        {
            // Presence/length are already enforced by [Required]/[MinLength].
            return true;
        }

        var hasUpper = password.Any(char.IsUpper);
        var hasLower = password.Any(char.IsLower);
        var hasDigit = password.Any(char.IsDigit);
        var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

        return hasUpper && hasLower && hasDigit && hasSpecial;
    }
}
