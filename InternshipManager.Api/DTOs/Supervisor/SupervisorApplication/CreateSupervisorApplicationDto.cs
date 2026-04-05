using System.ComponentModel.DataAnnotations;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.DTOs.SupervisorApplication;

public class CreateSupervisorApplicationDto
{
    // IdEmployee не в DTO! Он берётся из URL или из токена
    
    // Кто создал (если null - значит создал сам руководитель)
    public Guid? IdCreatedBy { get; set; }
    
    // Если выбрана практика из расписания - эти поля могут быть не заполнены
    public int? IdScheduledPractice { get; set; }
    
    // Если НЕ выбрана практика из расписания - эти поля ОБЯЗАТЕЛЬНЫ
    public int? IdSpecialization { get; set; }
    public int? IdDepartment { get; set; }
    public int? IdAddress { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // Всегда обязательны
    [Required]
    [Range(1, 100, ErrorMessage = "Количество практикантов должно быть от 1 до 100")]
    public int RequestedStudentsCount { get; set; }
    
    [Required]
    public PracticeFormat PracticeFormat { get; set; } = PracticeFormat.Очная;
    
    public bool IsPaid { get; set; } = false;
}