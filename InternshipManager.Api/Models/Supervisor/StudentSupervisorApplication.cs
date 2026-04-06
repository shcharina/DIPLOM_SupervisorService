using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Supervisor;

[PrimaryKey(nameof(IdSupervisorApplication), nameof(IdStudentApplication))]
public class StudentSupervisorApplication
{
    public Guid IdSupervisorApplication { get; set; }
    
    public Guid IdStudentApplication { get; set; }  // ID заявки студента (из другой системы)
    
    public StudentApplicationStatus Status { get; set; } = StudentApplicationStatus.НаРассмотренииРуководителем;
    
    public DateTime? AssignedAt { get; set; }
    
    public DateTime? UpdatedAt { get; set; }
    
    // Навигационные свойства
    [ForeignKey(nameof(IdSupervisorApplication))]
    public SupervisorApplication? SupervisorApplication { get; set; }
}