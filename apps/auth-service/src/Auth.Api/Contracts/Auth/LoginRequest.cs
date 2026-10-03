using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Contracts.Auth;

public sealed record LoginRequest(
    [Required, EmailAddress, MaxLength(256)]
    string Email,

    [Required]
    string Password
);