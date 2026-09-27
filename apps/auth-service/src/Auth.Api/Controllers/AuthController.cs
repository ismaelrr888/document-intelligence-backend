using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Auth.Api.Contracts.Auth;
using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(
            request.Email,
            request.Password);

        if (!result)
        {
            return Conflict();
        }

        return Ok();
    }

    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Succeeded)
        {
            // Generic message: never reveal whether the user exists or the
            // password was wrong, to avoid user enumeration.
            return Unauthorized(new ErrorResponse("Invalid credentials"));
        }

        return Ok(new LoginResponse(result.Token!, result.ExpiresAtUtc!.Value));
    }

    [Authorize]
    [ProducesResponseType(typeof(MeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var email = User.FindFirstValue(JwtRegisteredClaimNames.Email);

        if (userId is null || email is null)
        {
            return Unauthorized();
        }

        return Ok(new MeResponse(Guid.Parse(userId), email));
    }
}