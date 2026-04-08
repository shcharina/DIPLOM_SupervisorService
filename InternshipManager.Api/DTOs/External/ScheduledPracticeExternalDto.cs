namespace InternshipManager.Api.DTOs.External;

public class ScheduledPracticeExternalDto
{
    public int IdScheduledPractice { get; set; }
    public int IdSpecialization { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

