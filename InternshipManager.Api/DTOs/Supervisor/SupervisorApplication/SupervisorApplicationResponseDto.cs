using InternshipManager.Api.Enums;
using InternshipManager.Api.DTOs.Shared;

namespace InternshipManager.Api.DTOs.SupervisorApplication;

public class SupervisorApplicationResponseDto
{
    // Основные поля
    public Guid IdSupervisorApplication { get; set; }
    
    // Руководитель (кто отвечает за практику)
    public Guid IdEmployee { get; set; }
    public EmployeeInfoDto? EmployeeInfo { get; set; }
    
    // Кто создал (может быть менеджер)
    public Guid? IdCreatedBy { get; set; }
    public EmployeeInfoDto? CreatedByInfo { get; set; }
    
    // Специализация
    public int IdSpecialization { get; set; }
    public SpecializationInfoDto? SpecializationInfo { get; set; }
    
    // Подразделение
    public int IdDepartment { get; set; }
    public DepartmentInfoDto? DepartmentInfo { get; set; }
    
    // Адрес
    public int IdAddress { get; set; }
    public AddressInfoDto? AddressInfo { get; set; }
    
    // Практика из расписания (если есть)
    public int? IdScheduledPractice { get; set; }
    public ScheduledPracticeInfoDto? ScheduledPracticeInfo { get; set; }
    
    // Даты (финальные)
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Количество студентов
    public int RequestedStudentsCount { get; set; }
    public int CurrentStudentsCount { get; set; }  // Из StudentSupervisorApplication
    
    // Дополнительные поля
    public PracticeFormat PracticeFormat { get; set; }
    public bool IsPaid { get; set; }
    public SupervisorApplicationStatus Status { get; set; }
    
    // Системные поля
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Вычисляемые поля
    public bool IsCreatedByManager => IdCreatedBy.HasValue && IdCreatedBy != IdEmployee;
    public bool IsFromSchedule => IdScheduledPractice.HasValue;
    public bool IsActive => Status == SupervisorApplicationStatus.Отправлена;
    public int AvailableSlots => RequestedStudentsCount - CurrentStudentsCount;
}