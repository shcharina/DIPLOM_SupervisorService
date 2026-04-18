namespace InternshipManager.Api.DTOs.External;

public class EmployeeExternalDto
{
    public EmployeeId IdEmployee { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string Position { get; set; } = string.Empty;
    public int Role { get; set; }  // 1 = Supervisor, 2 = Manager
    public string? Phone { get; set; }
    public string? Email { get; set; }
}