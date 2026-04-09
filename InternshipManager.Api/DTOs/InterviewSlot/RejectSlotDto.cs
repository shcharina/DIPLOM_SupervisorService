using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.DTOs.InterviewSlot;

public class RejectSlotDto
{
    [Required]
    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;
}
