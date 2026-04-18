using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.DTOs.Auth;

public class LoginDto
{
    [Required]
    public string Login { get; set; } = string.Empty;
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
}