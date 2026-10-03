using System.ComponentModel.DataAnnotations;
using Auth.Api.Validation;

namespace Auth.Api.Contracts.Auth;

public sealed record RegisterRequest(
    [Required, EmailAddress, MaxLength(256)]
    string Email,

    [Required, MinLength(8), MaxLength(128), StrongPassword]
    string Password
);
