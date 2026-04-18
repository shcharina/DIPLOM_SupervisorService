using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

using InternshipManager.Api.DTOs.Auth;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Services;
public class AuthService : IAuthService
{
    private readonly ManagerApiClient _managerApi;
    private readonly IConfiguration _configuration;
    public AuthService(ManagerApiClient managerApi, IConfiguration configuration)
    {
        _managerApi = managerApi;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        // 1. Проверяем логин/пароль через Manager API
        var employee = await _managerApi.LoginAsync(dto.Login, dto.PasswordHash);

        if (employee == null)
            return null;

        // 2. Проверяем, что это руководитель
        if (employee.Role != 1)
            return null;

        // 3. Генерируем JWT-токен
        var token = GenerateToken(employee);
        return new LoginResponseDto
        {
            Token = token,
            EmployeeId = employee.IdEmployee,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Patronymic = employee.Patronymic,
            Position = employee.Position
        };
    }

    private string GenerateToken(DTOs.External.EmployeeExternalDto employee)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, employee.IdEmployee.ToString()),
            new Claim(ClaimTypes.Name, $"{employee.LastName} {employee.FirstName}"),
            new Claim(ClaimTypes.Role, "Supervisor")
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(
                double.Parse(_configuration["Jwt:ExpiresInHours"] ?? "8")),
            signingCredentials: new SigningCredentials(
                key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}