using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Supervisor;

public class SupervisorApplication
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // убрать при возврате GUid
    public SupervisorApplicationId IdSupervisorApplication { get; set; } // = Guid.NewGuid();
    
    [Required]
    public EmployeeId IdEmployee { get; set; }  // Руководитель, отвечающий за практику в заявке
    
    public EmployeeId? IdCreatedBy { get; set; }  // Кто создал (Employee)
    
    // FK на общую БД (храним только ID)
    [Required]
    public SpecializationId IdSpecialization { get; set; }
    [Required]
    public DepartmentId IdDepartment { get; set; }
    [Required]
    public AddressId IdAddress { get; set; }
    public ScheduledPracticeId? IdScheduledPractice { get; set; }  // Если выбрана практика из расписания
    
    [Required]
    public int RequestedStudentsCount { get; set; }
    
    public DateTime? StartDate { get; set; }   // Если не из расписания - заполняется вручную
    public DateTime? EndDate { get; set; }     // Если не из расписания - заполняется вручную
    
    public PracticeFormat PracticeFormat { get; set; } = PracticeFormat.Очная;
    
    public bool IsPaid { get; set; } = false;
    
    public SupervisorApplicationStatus Status { get; set; } = SupervisorApplicationStatus.Шаблон;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UpdatedAt { get; set; }
    
    // Навигационные свойства (только для SupervisorApplication)
    public ICollection<StudentSupervisorApplication> StudentSupervisorApplications { get; set; } = new List<StudentSupervisorApplication>();
}