using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.DTOs.InterviewSlot;

public class BookSlotDto
{
    [Required]
    public StudentApplicationId IdStudentApplication { get; set; }
}