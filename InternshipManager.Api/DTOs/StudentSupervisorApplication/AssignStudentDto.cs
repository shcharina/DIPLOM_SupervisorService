using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.DTOs.StudentSupervisorApplication;

public class AssignStudentDto
{
    [Required]
    public SupervisorApplicationId IdSupervisorApplication { get; set; }

    [Required]
    public StudentApplicationId IdStudentApplication { get; set; }
}