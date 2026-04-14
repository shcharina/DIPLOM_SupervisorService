using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Supervisor;

[PrimaryKey(nameof(IdSupervisorApplication), nameof(IdStudentApplication))]
public class StudentSupervisorApplication
{
    public SupervisorApplicationId IdSupervisorApplication { get; set; }
    
    public StudentApplicationId IdStudentApplication { get; set; }  // ID заявки студента (из другой системы)
    
    public StudentSupervisorApplicationStatus Status { get; set; } = StudentSupervisorApplicationStatus.НаРассмотренииРуководителем;

    // Навигационные свойства
    [ForeignKey(nameof(IdSupervisorApplication))]
    public SupervisorApplication? SupervisorApplication { get; set; }
}