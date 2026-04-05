using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using InternshipManager.Api.Data;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.DTOs.SupervisorApplication;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Services;

public class SupervisorApplicationService : ISupervisorApplicationService
{
    private readonly SupervisorDbContext _supervisorContext;
    private readonly ILogger<SupervisorApplicationService> _logger;
    
    public SupervisorApplicationService(
        SupervisorDbContext supervisorContext,
        ILogger<SupervisorApplicationService> logger)
    {
        _supervisorContext = supervisorContext;
        _logger = logger;
    }
    
    // ========== ВАЛИДАЦИЯ ==========
    
    public async Task ValidateDatesAsync(DateTime? startDate, DateTime? endDate, int? idScheduledPractice)
    {
        // Если есть практика из расписания - даты не нужны
        if (idScheduledPractice.HasValue)
        {
            return;
        }
        
        // Если нет практики из расписания - обе даты обязательны
        if (!startDate.HasValue || !endDate.HasValue)
        {
            throw new ValidationException("Даты начала и окончания практики обязательны, если не выбрана практика из расписания");
        }
        
        // Начало не может быть позже конца
        if (startDate.Value >= endDate.Value)
        {
            throw new ValidationException("Дата начала не может быть позже или равна дате окончания");
        }
        
        // Дата начала не может быть в прошлом (кроме черновиков, но проверка при отправке)
        if (startDate.Value.Date < DateTime.UtcNow.Date)
        {
            throw new ValidationException("Дата начала не может быть в прошлом");
        }
    }
    
    public async Task ValidateApplicationForSendingAsync(SupervisorApplication application)
    {
        var errors = new List<string>();
        
        // Проверка обязательных полей
        if (application.IdSpecialization == 0)
            errors.Add("Не выбрана специализация");
        
        if (application.IdDepartment == 0)
            errors.Add("Не выбрано подразделение");
        
        if (application.IdAddress == 0)
            errors.Add("Не выбран адрес");
        
        if (application.RequestedStudentsCount <= 0)
            errors.Add("Количество практикантов должно быть больше 0");
        
        // Проверка дат
        if (!application.IdScheduledPractice.HasValue)
        {
            if (!application.StartDate.HasValue || !application.EndDate.HasValue)
            {
                errors.Add("Даты начала и окончания практики обязательны");
            }
            else if (application.StartDate.Value >= application.EndDate.Value)
            {
                errors.Add("Дата начала не может быть позже даты окончания");
            }
        }
        
        if (errors.Any())
        {
            throw new ValidationException($"Невозможно отправить заявку: {string.Join("; ", errors)}");
        }
    }
    
    // ========== ОСНОВНЫЕ ОПЕРАЦИИ ==========
    
    public async Task<SupervisorApplication> CreateApplicationAsync(Guid supervisorId, CreateSupervisorApplicationDto dto)
    {
        // Валидация дат
        await ValidateDatesAsync(dto.StartDate, dto.EndDate, dto.IdScheduledPractice);
        
        var application = new SupervisorApplication
        {
            IdSupervisorApplication = Guid.NewGuid(),
            IdEmployee = supervisorId,
            IdCreatedBy = dto.IdCreatedBy,
            IdScheduledPractice = dto.IdScheduledPractice,
            RequestedStudentsCount = dto.RequestedStudentsCount,
            PracticeFormat = dto.PracticeFormat,
            IsPaid = dto.IsPaid,
            Status = SupervisorApplicationStatus.Шаблон,
            CreatedAt = DateTime.UtcNow
        };
        
        // Если выбрана практика из расписания - остальные поля не нужны
        if (dto.IdScheduledPractice.HasValue)
        {
            // IdSpecialization, IdDepartment, IdAddress, StartDate, EndDate 
            // будут заполнены из ScheduledPractice при отправке или получении
            application.IdSpecialization = 0;  // Временное значение
            application.IdDepartment = 0;
            application.IdAddress = 0;
        }
        else
        {
            // Обязательные поля для ручного ввода
            if (!dto.IdSpecialization.HasValue || !dto.IdDepartment.HasValue || !dto.IdAddress.HasValue)
            {
                throw new ValidationException("Специализация, подразделение и адрес обязательны");
            }
            
            application.IdSpecialization = dto.IdSpecialization.Value;
            application.IdDepartment = dto.IdDepartment.Value;
            application.IdAddress = dto.IdAddress.Value;
            application.StartDate = dto.StartDate;
            application.EndDate = dto.EndDate;
        }
        
        _supervisorContext.SupervisorApplications.Add(application);
        await _supervisorContext.SaveChangesAsync();
        
        _logger.LogInformation("Создана заявка {ApplicationId} руководителем {SupervisorId}", 
            application.IdSupervisorApplication, supervisorId);
        
        return application;
    }
    
    public async Task<SupervisorApplication?> UpdateApplicationAsync(Guid applicationId, UpdateSupervisorApplicationDto dto)
    {
        var application = await _supervisorContext.SupervisorApplications
            .FirstOrDefaultAsync(sa => sa.IdSupervisorApplication == applicationId);
        
        if (application == null)
            return null;
        
        // Только черновики можно редактировать
        if (application.Status != SupervisorApplicationStatus.Шаблон)
        {
            throw new ValidationException($"Нельзя редактировать заявку в статусе {application.Status}");
        }
        
        // Обновляем только переданные поля
        if (dto.IdSpecialization.HasValue)
            application.IdSpecialization = dto.IdSpecialization.Value;
        
        if (dto.IdDepartment.HasValue)
            application.IdDepartment = dto.IdDepartment.Value;
        
        if (dto.IdAddress.HasValue)
            application.IdAddress = dto.IdAddress.Value;
        
        if (dto.IdScheduledPractice.HasValue)
            application.IdScheduledPractice = dto.IdScheduledPractice.Value;
        
        if (dto.StartDate.HasValue)
            application.StartDate = dto.StartDate.Value;
        
        if (dto.EndDate.HasValue)
            application.EndDate = dto.EndDate.Value;
        
        if (dto.RequestedStudentsCount.HasValue)
            application.RequestedStudentsCount = dto.RequestedStudentsCount.Value;
        
        if (dto.PracticeFormat.HasValue)
            application.PracticeFormat = dto.PracticeFormat.Value;
        
        if (dto.IsPaid.HasValue)
            application.IsPaid = dto.IsPaid.Value;
        
        application.UpdatedAt = DateTime.UtcNow;
        
        // Повторная валидация дат
        await ValidateDatesAsync(application.StartDate, application.EndDate, application.IdScheduledPractice);
        
        await _supervisorContext.SaveChangesAsync();
        
        return application;
    }
    
    public async Task<bool> DeleteApplicationAsync(Guid applicationId)
    {
        var application = await _supervisorContext.SupervisorApplications
            .FirstOrDefaultAsync(sa => sa.IdSupervisorApplication == applicationId);
        
        if (application == null)
            return false;
        
        // Только черновики можно удалять
        if (application.Status != SupervisorApplicationStatus.Шаблон)
        {
            throw new ValidationException($"Нельзя удалить заявку в статусе {application.Status}");
        }
        
        _supervisorContext.SupervisorApplications.Remove(application);
        await _supervisorContext.SaveChangesAsync();
        
        return true;
    }
    
    // ========== ИЗМЕНЕНИЕ СТАТУСА ==========
    
    public async Task<bool> SendApplicationAsync(Guid applicationId)
    {
        var application = await _supervisorContext.SupervisorApplications
            .FirstOrDefaultAsync(sa => sa.IdSupervisorApplication == applicationId);
        
        if (application == null)
            return false;
        
        if (application.Status != SupervisorApplicationStatus.Шаблон)
        {
            throw new ValidationException($"Нельзя отправить заявку в статусе {application.Status}");
        }
        
        // Валидация перед отправкой
        await ValidateApplicationForSendingAsync(application);
        
        application.Status = SupervisorApplicationStatus.Отправлена;
        application.UpdatedAt = DateTime.UtcNow;
        
        await _supervisorContext.SaveChangesAsync();
        
        _logger.LogInformation("Заявка {ApplicationId} отправлена", applicationId);
        
        return true;
    }
    
    public async Task<bool> CancelApplicationAsync(Guid applicationId)
    {
        var application = await _supervisorContext.SupervisorApplications
            .Include(sa => sa.StudentSupervisorApplications)
            .FirstOrDefaultAsync(sa => sa.IdSupervisorApplication == applicationId);
        
        if (application == null)
            return false;
        
        if (application.Status != SupervisorApplicationStatus.Отправлена && 
            application.Status != SupervisorApplicationStatus.Шаблон)
        {
            throw new ValidationException($"Нельзя отменить заявку в статусе {application.Status}");
        }
        
        // Открепляем всех студентов
        foreach (var studentApp in application.StudentSupervisorApplications)
        {
            studentApp.Status = StudentApplicationStatus.Отказано;
            studentApp.UpdatedAt = DateTime.UtcNow;
        }
        
        application.Status = SupervisorApplicationStatus.Отменена;
        application.UpdatedAt = DateTime.UtcNow;
        
        await _supervisorContext.SaveChangesAsync();
        
        _logger.LogInformation("Заявка {ApplicationId} отменена, откреплено {Count} студентов", 
            applicationId, application.StudentSupervisorApplications.Count);
        
        return true;
    }
    
    public async Task<bool> CloseApplicationAsync(Guid applicationId)
    {
        var application = await _supervisorContext.SupervisorApplications
            .FirstOrDefaultAsync(sa => sa.IdSupervisorApplication == applicationId);
        
        if (application == null)
            return false;
        
        if (application.Status != SupervisorApplicationStatus.Отправлена)
        {
            throw new ValidationException($"Нельзя закрыть заявку в статусе {application.Status}");
        }
        
        application.Status = SupervisorApplicationStatus.Закрыта;
        application.UpdatedAt = DateTime.UtcNow;
        
        await _supervisorContext.SaveChangesAsync();
        
        return true;
    }
    
    // ========== ПРОВЕРКА СТАТУСОВ ==========
    
    public async Task CheckAndUpdateSatisfiedStatusAsync(Guid applicationId)
    {
        var application = await _supervisorContext.SupervisorApplications
            .Include(sa => sa.StudentSupervisorApplications)
            .FirstOrDefaultAsync(sa => sa.IdSupervisorApplication == applicationId);
        
        if (application == null || application.Status != SupervisorApplicationStatus.Отправлена)
            return;
        
        var currentStudentsCount = application.StudentSupervisorApplications
            .Count(ssa => ssa.Status == StudentApplicationStatus.Принят || 
                          ssa.Status == StudentApplicationStatus.ОформлениеДокументов);
        
        if (currentStudentsCount >= application.RequestedStudentsCount)
        {
            application.Status = SupervisorApplicationStatus.Удовлетворена;
            application.UpdatedAt = DateTime.UtcNow;
            await _supervisorContext.SaveChangesAsync();
            
            _logger.LogInformation("Заявка {ApplicationId} удовлетворена (набрано {Count} студентов)", 
                applicationId, currentStudentsCount);
        }
    }
    
    public async Task CheckAndUpdateUnsatisfiedStatusAsync()
    {
        var today = DateTime.UtcNow.Date;
        
        var unsatisfiedApplications = await _supervisorContext.SupervisorApplications
            .Include(sa => sa.StudentSupervisorApplications)
            .Where(sa => sa.Status == SupervisorApplicationStatus.Отправлена &&
                         sa.StartDate.HasValue &&
                         sa.StartDate.Value.Date <= today)
            .ToListAsync();
        
        foreach (var application in unsatisfiedApplications)
        {
            var currentStudentsCount = application.StudentSupervisorApplications
                .Count(ssa => ssa.Status == StudentApplicationStatus.Принят);
            
            if (currentStudentsCount < application.RequestedStudentsCount)
            {
                application.Status = SupervisorApplicationStatus.Неудовлетворена;
                application.UpdatedAt = DateTime.UtcNow;
                
                _logger.LogWarning("Заявка {ApplicationId} не удовлетворена - дата начала {StartDate} наступила, а набрано {Current} из {Required} студентов",
                    application.IdSupervisorApplication, application.StartDate, currentStudentsCount, application.RequestedStudentsCount);
            }
        }
        
        await _supervisorContext.SaveChangesAsync();
    }
    
    // ========== ПОЛУЧЕНИЕ ДАННЫХ ==========
    
    public async Task<SupervisorApplication?> GetApplicationByIdAsync(Guid applicationId)
    {
        return await _supervisorContext.SupervisorApplications
            .Include(sa => sa.StudentSupervisorApplications)
            .FirstOrDefaultAsync(sa => sa.IdSupervisorApplication == applicationId);
    }
    
    public async Task<IEnumerable<SupervisorApplication>> GetApplicationsBySupervisorAsync(Guid supervisorId)
    {
        return await _supervisorContext.SupervisorApplications
            .Where(sa => sa.IdEmployee == supervisorId)
            .Include(sa => sa.StudentSupervisorApplications)
            .OrderByDescending(sa => sa.CreatedAt)
            .ToListAsync();
    }
    
    public async Task<IEnumerable<SupervisorApplication>> GetActiveApplicationsAsync()
    {
        return await _supervisorContext.SupervisorApplications
            .Where(sa => sa.Status == SupervisorApplicationStatus.Отправлена)
            .Include(sa => sa.StudentSupervisorApplications)
            .OrderBy(sa => sa.StartDate)
            .ToListAsync();
    }
}