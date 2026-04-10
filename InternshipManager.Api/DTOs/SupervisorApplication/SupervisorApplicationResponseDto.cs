
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.DTOs.SupervisorApplication;

//Что уходит клиенту в ответе

public class SupervisorApplicationResponseDto
{
    public SupervisorApplicationId IdSupervisorApplication { get; set; }

    public EmployeeId IdEmployee { get; set; }

    public EmployeeId? IdCreatedBy { get; set; }

    public SpecializationId IdSpecialization { get; set; }

    public DepartmentId IdDepartment { get; set; }

    public AddressId IdAddress { get; set; }

    public ScheduledPracticeId? IdScheduledPractice { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int RequestedStudentsCount { get; set; }

    public PracticeFormat PracticeFormat { get; set; }

    public bool IsPaid { get; set; }

    public SupervisorApplicationStatus Status { get; set; }

    // Вычисляемые поля

    public bool IsCreatedByManager { get; set; }

    public bool IsFromSchedule { get; set; }

}