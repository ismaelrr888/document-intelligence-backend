namespace Auth.Api.Contracts.Auth;

public sealed record RefreshRequest(
    string RefreshToken
);
