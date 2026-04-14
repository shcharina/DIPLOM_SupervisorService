using System.ComponentModel.DataAnnotations;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.DTOs.SupervisorApplication;

public class CreateSupervisorApplicationDto
{
    [Required]
    public EmployeeId SupervisorId { get; set; }          // кто руководитель

    public ScheduledPracticeId? IdScheduledPractice { get; set; }   // практика из расписания (необязательно)

    public SpecializationId? IdSpecialization { get; set; }      // специализация (если не из расписания)

    [Required]
    public DepartmentId IdDepartment { get; set; }           // подразделение

    [Required]
    public AddressId IdAddress { get; set; }              // адрес

    public DateTime? StartDate { get; set; }        // дата начала (если не из расписания)

    public DateTime? EndDate { get; set; }          // дата конца (если не из расписания)

    [Range(1, 100)]
    public int RequestedStudentsCount { get; set; } = 1;

    [Required]
    public PracticeFormat PracticeFormat { get; set; }

    public bool IsPaid { get; set; } = false;

}