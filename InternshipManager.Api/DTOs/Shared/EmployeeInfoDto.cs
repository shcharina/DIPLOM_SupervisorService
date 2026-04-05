namespace InternshipManager.Api.DTOs.Shared;

public class EmployeeInfoDto
{
    public Guid IdEmployee { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Patronymic { get; set; }
    public string Position { get; set; } = string.Empty;
    public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
    public string ShortName => $"{LastName} {FirstName?[0]}." + (Patronymic != null ? $"{Patronymic[0]}." : "");
}