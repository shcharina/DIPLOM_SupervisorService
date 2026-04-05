namespace InternshipManager.Api.DTOs.Shared;

public class SpecializationInfoDto
{
    public int IdSpecialization { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}