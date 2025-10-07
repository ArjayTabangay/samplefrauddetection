using LeadFraudDetection.Api.Auth;
using Microsoft.AspNetCore.Mvc;

namespace LeadFraudDetection.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtTokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(JwtTokenService tokenService, ILogger<AuthController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Email and password are required");
        }

        if (request.Email == "admin@example.com" && request.Password == "Admin123!")
        {
            var token = _tokenService.GenerateToken("1", request.Email, new[] { "Admin" });
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            return Ok(new { token });
        }

        if (request.Email == "user@example.com" && request.Password == "User123!")
        {
            var token = _tokenService.GenerateToken("2", request.Email, new[] { "User" });
            _logger.LogInformation("User {Email} logged in successfully", request.Email);
            return Ok(new { token });
        }

        _logger.LogWarning("Failed login attempt for {Email}", request.Email);
        return Unauthorized("Invalid credentials");
    }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
