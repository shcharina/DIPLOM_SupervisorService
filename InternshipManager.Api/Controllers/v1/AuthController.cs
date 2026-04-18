using Microsoft.AspNetCore.Mvc;

using InternshipManager.Api.DTOs.Auth;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result == null)
            return Unauthorized(new { detail = "Неверный логин, пароль или недостаточно прав" });

        return Ok(result);
    }
}