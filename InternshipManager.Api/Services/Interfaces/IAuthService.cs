using InternshipManager.Api.DTOs.Auth;
namespace InternshipManager.Api.Services.Interfaces;
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}