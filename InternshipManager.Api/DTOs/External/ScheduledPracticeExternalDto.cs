namespace InternshipManager.Api.DTOs.External;

public class ScheduledPracticeExternalDto
{
    public ScheduledPracticeId IdScheduledPractice { get; set; }
    public SpecializationId IdSpecialization { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

