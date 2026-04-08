using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManager.Api.Models.Shared;

public class ScheduledPractice
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // убрать если GUid
    public ScheduledPracticeId IdScheduledPractice { get; set; }
    
    public SpecializationId IdSpecialization { get; set; }
    
    public PracticeTypeId IdPracticeType { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    // Навигационные свойства
    [ForeignKey(nameof(IdSpecialization))]
    public Specialization? Specialization { get; set; }
    
    [ForeignKey(nameof(IdPracticeType))]
    public PracticeType? PracticeType { get; set; }
}