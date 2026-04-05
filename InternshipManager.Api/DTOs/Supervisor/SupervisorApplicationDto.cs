using System.ComponentModel.DataAnnotations;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.DTOs.Supervisor;

// Для создания новой заявки
public class CreateSupervisorApplicationDto
{
    // IdEmployee берётся из токена/URL, не из DTO
    
    public Guid? IdCreatedBy { get; set; }  // Кто создал (менеджер)
    
    [Required]
    public int IdSpecialization { get; set; }
    
    [Required]
    public int IdDepartment { get; set; }
    
    [Required]
    public int IdAddress { get; set; }
    
    public int? IdScheduledPractice { get; set; }  // Из расписания
    
    [Required]
    [Range(1, 100)]
    public int RequestedStudentsCount { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public PracticeFormat PracticeFormat { get; set; } = PracticeFormat.Очная;
    public bool IsPaid { get; set; } = false;
}

// Для обновления заявки (черновика)
public class UpdateSupervisorApplicationDto
{
    public int? IdSpecialization { get; set; }
    public int? IdDepartment { get; set; }
    public int? IdAddress { get; set; }
    public int? IdScheduledPractice { get; set; }
    public int? RequestedStudentsCount { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public PracticeFormat? PracticeFormat { get; set; }
    public bool? IsPaid { get; set; }
}

// Для ответа (GET)
public class SupervisorApplicationResponseDto
{
    public Guid IdSupervisorApplication { get; set; }
    public Guid IdEmployee { get; set; }
    public string EmployeeName { get; set; } = string.Empty;  // Из общей БД
    public string EmployeePosition { get; set; } = string.Empty;
    public Guid? IdCreatedBy { get; set; }
    public string? CreatorName { get; set; }
    
    public int IdSpecialization { get; set; }
    public string SpecializationName { get; set; } = string.Empty;
    
    public int IdDepartment { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    
    public int IdAddress { get; set; }
    public string FullAddress { get; set; } = string.Empty;
    
    public int? IdScheduledPractice { get; set; }
    public DateTime? ScheduledStartDate { get; set; }
    public DateTime? ScheduledEndDate { get; set; }
    
    public int RequestedStudentsCount { get; set; }
    public int CurrentStudentsCount { get; set; }  // Уже прикреплено
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public string PracticeFormat { get; set; } = string.Empty;
    public bool IsPaid { get; set; }
    public string Status { get; set; } = string.Empty;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

// Для отправки/отмены/закрытия заявки
public class ApplicationActionResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ApplicationId { get; set; }
    public string NewStatus { get; set; } = string.Empty;
}