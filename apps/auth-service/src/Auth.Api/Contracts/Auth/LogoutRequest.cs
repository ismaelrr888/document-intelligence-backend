namespace Auth.Api.Contracts.Auth;

public sealed record LogoutRequest(
    string RefreshToken
);
