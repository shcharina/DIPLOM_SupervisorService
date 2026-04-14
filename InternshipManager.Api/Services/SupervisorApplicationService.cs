using InternshipManager.Api.DTOs.SupervisorApplication;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Services;

public class SupervisorApplicationService : ISupervisorApplicationService
{
    private readonly ISupervisorApplicationRepository _repository;
    private readonly ManagerApiClient _managerApi;
    public SupervisorApplicationService(
        ISupervisorApplicationRepository repository,
        ManagerApiClient managerApi)
    {
        _repository = repository;
        _managerApi = managerApi;
    }

    public async Task<SupervisorApplication?> GetByIdAsync(SupervisorApplicationId id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<(List<SupervisorApplication> Data, int TotalItems)> 
        GetBySupervisorAsync(
            EmployeeId supervisorId, int page, int pageSize,
            SupervisorApplicationStatus? status)
    {
        return await _repository.GetBySupervisorAsync(
            supervisorId, page, pageSize, status);
    }

    public async Task<(List<SupervisorApplication> Data, int TotalItems)> 
        GetActiveAsync(int page, int pageSize)
    {
        return await _repository.GetActiveAsync(page, pageSize);
    }

    public async Task<List<SupervisorApplication>> GetActivePracticesAsync(
        EmployeeId supervisorId)
    {
        return await _repository.GetActivePracticesAsync(supervisorId);
    }

    public async Task<SupervisorApplicationResponseDto> CreateAsync(
        CreateSupervisorApplicationDto dto)
    {
        // === Бизнес-логика: проверка руководителя ===
        var supervisor = await _managerApi.GetSupervisorByIdAsync(dto.SupervisorId);
        if (supervisor == null || supervisor.Role != 1)
            throw new InvalidOperationException("Руководитель с таким ID не найден");

        // === Бизнес-логика: определение дат и специализации ===
        SpecializationId idSpecialization;
        DateTime? startDate;
        DateTime? endDate;

        if (dto.IdScheduledPractice.HasValue)
        {
            var scheduled = await _managerApi
                .GetScheduledPracticeAsync(dto.IdScheduledPractice.Value);

            if (scheduled == null)
                throw new InvalidOperationException(
                    "Практика из расписания не найдена");

            idSpecialization = scheduled.IdSpecialization;
            startDate = scheduled.StartDate;
            endDate = scheduled.EndDate;
        }
        else
        {
            if (dto.IdSpecialization == null || 
                dto.StartDate == null || 
                dto.EndDate == null)
                throw new ArgumentException(
                    "Укажите специализацию, дату начала и дату конца");

            if (dto.StartDate >= dto.EndDate)
                throw new ArgumentException(
                    "Дата окончания должна быть позже даты начала");

            idSpecialization = dto.IdSpecialization.Value;
            startDate = dto.StartDate;
            endDate = dto.EndDate;

        }

        // === Создание сущности ===

        var application = new SupervisorApplication
        {
            IdEmployee = dto.SupervisorId,
            IdSpecialization = idSpecialization,
            IdDepartment = dto.IdDepartment,
            IdAddress = dto.IdAddress,
            IdScheduledPractice = dto.IdScheduledPractice,
            StartDate = startDate,
            EndDate = endDate,
            RequestedStudentsCount = dto.RequestedStudentsCount,
            PracticeFormat = dto.PracticeFormat,
            IsPaid = dto.IsPaid,
            Status = SupervisorApplicationStatus.Шаблон,
        };

        await _repository.AddAsync(application);
        return ToResponseDto(application);
    }

    public async Task<SupervisorApplicationResponseDto> UpdateAsync(
        SupervisorApplicationId id, UpdateSupervisorApplicationDto dto)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
            throw new KeyNotFoundException("Заявка не найдена");

        if (application.Status != SupervisorApplicationStatus.Шаблон)
            throw new InvalidOperationException(
                $"Нельзя редактировать заявку в статусе {application.Status}");

        // Обновление полей

        if (dto.IdSpecialization.HasValue) 
            application.IdSpecialization = dto.IdSpecialization.Value;

        if (dto.IdDepartment.HasValue) 
            application.IdDepartment = dto.IdDepartment.Value;

        if (dto.IdAddress.HasValue) 
            application.IdAddress = dto.IdAddress.Value;

        if (dto.IdScheduledPractice.HasValue) 
            application.IdScheduledPractice = dto.IdScheduledPractice;

        if (dto.RequestedStudentsCount.HasValue) 
            application.RequestedStudentsCount = dto.RequestedStudentsCount.Value;

        if (dto.PracticeFormat.HasValue) 
            application.PracticeFormat = dto.PracticeFormat.Value;

        if (dto.IsPaid.HasValue) 
            application.IsPaid = dto.IsPaid.Value;

        if (dto.StartDate.HasValue) 
            application.StartDate = dto.StartDate;

        if (dto.EndDate.HasValue) 
            application.EndDate = dto.EndDate;

        await _repository.UpdateAsync(application);
        return ToResponseDto(application);
    }

    public async Task DeleteAsync(SupervisorApplicationId id)
    {
        var application = await _repository.GetByIdAsync(id);

        if (application == null)
            throw new KeyNotFoundException("Заявка не найдена");

        if (application.Status != SupervisorApplicationStatus.Шаблон)
            throw new InvalidOperationException(
                "Удалить можно только заявку в статусе Шаблон");

        await _repository.DeleteAsync(application);
    }

    public async Task<object> SendAsync(SupervisorApplicationId id)
    {
        var application = await _repository.GetByIdAsync(id);

        if (application == null)
            throw new KeyNotFoundException("Заявка не найдена");

        if (application.Status != SupervisorApplicationStatus.Шаблон)
            throw new InvalidOperationException(
                $"Невозможно отправить заявку в статусе {application.Status}");

        application.Status = SupervisorApplicationStatus.Отправлена;
        await _repository.UpdateAsync(application);

        return new
        {
            idSupervisorApplication = application.IdSupervisorApplication,
            status = application.Status,
            message = "Заявка успешно отправлена"
        };
    }

    public async Task<object> CloseAsync(SupervisorApplicationId id)
    {
        var application = await _repository.GetByIdAsync(id);
        if (application == null)
            throw new KeyNotFoundException("Заявка не найдена");

        if (application.Status != SupervisorApplicationStatus.Отправлена)
            throw new InvalidOperationException(
                $"Нельзя закрыть заявку в статусе {application.Status}");

        application.Status = SupervisorApplicationStatus.Закрыта;
        await _repository.UpdateAsync(application);
        return new
        {
            idSupervisorApplication = application.IdSupervisorApplication,
            status = application.Status,
            message = "Заявка закрыта досрочно"
        };
    }

    public async Task<object> CancelAsync(SupervisorApplicationId id)
    {
        /*  Нужен ещё репозиторий студентов и слотов
            Пока заглушка — реализовать когда 
            будут IStudentSupervisorApplicationRepository
            и IInterviewSlotRepository
        */
        throw new NotImplementedException();
    }

    // === Маппинг ===

    private static SupervisorApplicationResponseDto ToResponseDto(
        SupervisorApplication app)
    {
        return new SupervisorApplicationResponseDto
        {
            IdSupervisorApplication = app.IdSupervisorApplication,
            IdEmployee = app.IdEmployee,
            IdCreatedBy = app.IdCreatedBy,
            IdSpecialization = app.IdSpecialization,
            IdDepartment = app.IdDepartment,
            IdAddress = app.IdAddress,
            IdScheduledPractice = app.IdScheduledPractice,
            StartDate = app.StartDate,
            EndDate = app.EndDate,
            RequestedStudentsCount = app.RequestedStudentsCount,
            PracticeFormat = app.PracticeFormat,
            IsPaid = app.IsPaid,
            Status = app.Status,
            IsCreatedByManager = app.IdCreatedBy != null 
                && app.IdCreatedBy != app.IdEmployee,
            IsFromSchedule = app.IdScheduledPractice != null
        };
    }
}