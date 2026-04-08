using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManager.Api.Models.Supervisor;

public class TimeInterval
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // убрать если GUid
    public IntervalId IdInterval { get; set; }
    
    [Required]
    public EmployeeId IdEmployee { get; set; }  // Кто будет проводить собеседования
    
    public EmployeeId? IdCreator { get; set; }  // Кто создал (если null или равен IdEmployee - создал сам руководитель)
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    [Required]
    public int MaxCount { get; set; }  // Максимум студентов в этот интервал
    
    public TimeSpan? BreakDuration { get; set; }  // Перерыв между слотами
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Навигационные свойства
    public ICollection<InterviewSlot> InterviewSlots { get; set; } = new List<InterviewSlot>();
}