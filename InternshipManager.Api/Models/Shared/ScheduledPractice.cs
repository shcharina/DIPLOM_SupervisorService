using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManager.Api.Models.Shared;

public class ScheduledPractice
{
    [Key]
    public int IdScheduledPractice { get; set; }
    
    public int IdSpecialization { get; set; }
    
    public int IdPracticeType { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime EndDate { get; set; }
    
    // Навигационные свойства
    [ForeignKey(nameof(IdSpecialization))]
    public Specialization? Specialization { get; set; }
    
    [ForeignKey(nameof(IdPracticeType))]
    public PracticeType? PracticeType { get; set; }
}