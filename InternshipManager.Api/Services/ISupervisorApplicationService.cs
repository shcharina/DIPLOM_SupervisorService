using InternshipManager.Api.DTOs.SupervisorApplication;
using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Services;

public interface ISupervisorApplicationService
{
    // Создание и базовые операции
    Task<SupervisorApplication> CreateApplicationAsync(Guid supervisorId, CreateSupervisorApplicationDto dto);
    Task<SupervisorApplication?> UpdateApplicationAsync(Guid applicationId, UpdateSupervisorApplicationDto dto);
    Task<bool> DeleteApplicationAsync(Guid applicationId);  // Только для черновиков
    
    // Изменение статуса
    Task<bool> SendApplicationAsync(Guid applicationId);      // ШАБЛОН → ОТПРАВЛЕНА
    Task<bool> CancelApplicationAsync(Guid applicationId);    // → ОТМЕНЕНА (открепляет студентов)
    Task<bool> CloseApplicationAsync(Guid applicationId);     // → ЗАКРЫТА (досрочно)
    
    // Проверка статусов (для фоновых задач)
    Task CheckAndUpdateSatisfiedStatusAsync(Guid applicationId);     // При наборе студентов
    Task CheckAndUpdateUnsatisfiedStatusAsync();                     // При наступлении даты начала
    
    // Получение данных
    Task<SupervisorApplication?> GetApplicationByIdAsync(Guid applicationId);
    Task<IEnumerable<SupervisorApplication>> GetApplicationsBySupervisorAsync(Guid supervisorId);
    Task<IEnumerable<SupervisorApplication>> GetActiveApplicationsAsync();
    
    // Валидация
    Task ValidateDatesAsync(DateTime? startDate, DateTime? endDate, int? idScheduledPractice);
    Task ValidateApplicationForSendingAsync(SupervisorApplication application);
}