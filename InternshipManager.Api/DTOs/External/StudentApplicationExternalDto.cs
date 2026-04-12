namespace InternshipManager.Api.DTOs.External;

public class StudentApplicationExternalDto
{
    public StudentApplicationId IdStudentApplication { get; set; }
    public int IdSpecialization { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Status { get; set; }
}