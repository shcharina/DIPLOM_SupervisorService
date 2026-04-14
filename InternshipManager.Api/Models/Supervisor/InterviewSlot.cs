using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Supervisor;

public class InterviewSlot
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // убрать если GUid
    public InterviewSlotId IdInterviewSlot { get; set; }
    
    [Required]
    public EmployeeId IdEmployee { get; set; }  // Кто будет проводить собеседование
    
    public EmployeeId? IdCreator { get; set; }  // Кто создал этот слот (null = сам руководитель)
    
    public IntervalId IdInterval { get; set; }  // FK на TimeInterval

    public SupervisorApplicationId? IdSupervisorApplication { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    public InterviewSlotStatus Status { get; set; } = InterviewSlotStatus.Черновик;
    
    [MaxLength(255)]
    public string? MeetingPlace { get; set; }
    
    [MaxLength(500)]
    public string? RejectionComment { get; set; } // В случае отказа руководителем от предложенного слота
    
    // Навигационные свойства
    [ForeignKey(nameof(IdInterval))]
    public TimeInterval? TimeInterval { get; set; }
    
    public Interview? Interview { get; set; }  // 1:1 связь
}