
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.DTOs.SupervisorApplication;

//Что уходит клиенту в ответе

public class SupervisorApplicationResponseDto

{

    public Guid IdSupervisorApplication { get; set; }

    public Guid IdEmployee { get; set; }

    public Guid? IdCreatedBy { get; set; }

    public int IdSpecialization { get; set; }

    public int IdDepartment { get; set; }

    public int IdAddress { get; set; }

    public int? IdScheduledPractice { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public int RequestedStudentsCount { get; set; }

    public PracticeFormat PracticeFormat { get; set; }

    public bool IsPaid { get; set; }

    public SupervisorApplicationStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Вычисляемые поля

    public bool IsCreatedByManager { get; set; }

    public bool IsFromSchedule { get; set; }

}


