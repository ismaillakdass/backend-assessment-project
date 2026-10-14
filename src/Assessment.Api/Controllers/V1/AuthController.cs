using Asp.Versioning;
using Assessment.Application.DTOs.Auth;
using Assessment.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Assessment.Api.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto, CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(dto, cancellationToken);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto, CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(dto, cancellationToken);
        return Ok(new { success = result, message = "Registration successful." });
    }

    /// <summary>
    /// TASK 11: JWT Logout / Token Revocation (5 Points)
    /// This endpoint responds 200 OK, but the token is not effectively revoked.
    /// The token can still be used to make authenticated requests after this endpoint is called!
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "No Bearer token found in Authorization header." });
        }

        var token = authHeader["Bearer ".Length..].Trim();
        await _authService.LogoutAsync(token, cancellationToken);

        return Ok(new { message = "Logout successful. Token revoked." });
    }
}
