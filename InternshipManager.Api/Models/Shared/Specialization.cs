using System.ComponentModel.DataAnnotations;

using InternshipManager.Api.Models.Manager;

namespace InternshipManager.Api.Models.Shared;

public class Specialization
{
    [Key]
    public int IdSpecialization { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    public string? Description { get; set; }
    
    // Навигационные свойства
    public ICollection<ScheduledPractice> ScheduledPractices { get; set; } = new List<ScheduledPractice>();

    // from danil
    public ICollection<DocumentForSpecialization> DocumentForSpecialization { get; set; } = new List<DocumentForSpecialization>();
}