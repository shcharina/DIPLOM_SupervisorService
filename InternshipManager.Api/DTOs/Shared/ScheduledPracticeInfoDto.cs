namespace InternshipManager.Api.DTOs.Shared;

public class ScheduledPracticeInfoDto
{
    public int IdScheduledPractice { get; set; }
    public int IdSpecialization { get; set; }
    public string SpecializationName { get; set; } = string.Empty;
    public int IdPracticeType { get; set; }
    public string PracticeTypeName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}