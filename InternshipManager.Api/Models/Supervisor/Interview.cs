using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Supervisor;

public class Interview
{
    [Key]
    [ForeignKey(nameof(InterviewSlot))]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // убрать если GUid
    public InterviewSlotId IdInterviewSlot { get; set; }  // PK и FK одновременно (1:1)

    public StudentApplicationId IdStudentApplication { get; set; }

    public InterviewType InterviewType { get; set; } = InterviewType.Руководитель;
    
    [Required]
    public bool Result { get; set; }  // true - принят, false - отклонён
    
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Навигационные свойства
    public InterviewSlot? InterviewSlot { get; set; }
}