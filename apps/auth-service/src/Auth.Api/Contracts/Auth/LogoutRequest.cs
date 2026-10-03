using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Contracts.Auth;

public sealed record LogoutRequest(
    [Required]
    string RefreshToken
);
