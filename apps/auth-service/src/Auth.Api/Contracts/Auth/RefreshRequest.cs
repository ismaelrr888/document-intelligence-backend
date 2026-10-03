using System.ComponentModel.DataAnnotations;

namespace Auth.Api.Contracts.Auth;

public sealed record RefreshRequest(
    [Required]
    string RefreshToken
);
