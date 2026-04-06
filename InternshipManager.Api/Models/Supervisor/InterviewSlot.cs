using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Supervisor;

public class InterviewSlot
{
    [Key]
    public int IdInterviewSlot { get; set; }
    
    [Required]
    public Guid IdEmployee { get; set; }  // Кто будет проводить собеседование
    
    public Guid? IdCreator { get; set; }  // Кто создал этот слот (null = сам руководитель)
    
    public int IdInterval { get; set; }  // FK на TimeInterval

    public Guid? IdSupervisorApplication { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    public InterviewSlotStatus Status { get; set; } = InterviewSlotStatus.Черновик;
    
    [MaxLength(255)]
    public string? MeetingPlace { get; set; }
    
    [MaxLength(500)]
    public string? RejectionComment { get; set; } // В случае отказа руководителем от предложенного слота

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Навигационные свойства
    [ForeignKey(nameof(IdInterval))]
    public TimeInterval? TimeInterval { get; set; }
    
    public Interview? Interview { get; set; }  // 1:1 связь
}