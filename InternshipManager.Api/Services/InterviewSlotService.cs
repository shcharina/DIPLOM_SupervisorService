using InternshipManager.Api.DTOs.InterviewSlot;
using InternshipManager.Api.DTOs.TimeInterval;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;
using InternshipManager.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace InternshipManager.Api.Services;

public class InterviewSlotService : IInterviewSlotService
{
    private readonly IInterviewSlotRepository _repository;
    private readonly IStudentSupervisorApplicationRepository _studentSupervisorRepository;

    public InterviewSlotService(
        IInterviewSlotRepository repository,
        IStudentSupervisorApplicationRepository studentSupervisorRepository)
    {
        _repository = repository;
        _studentSupervisorRepository = studentSupervisorRepository;
    }

    public async Task<List<object>> GetPendingBySupervisorAsync(EmployeeId supervisorId)
    {
        var slots = await _repository.GetPendingBySupervisorAsync(supervisorId);
        return slots.Cast<object>().ToList();
    }

    public async Task<List<object>> GetBySupervisorAsync(EmployeeId supervisorId)
    {
        var slots = await _repository.GetBySupervisorAsync(supervisorId);
        return slots.Select(s => (object)new
        {
            idInterviewSlot = s.IdInterviewSlot,
            startTime = s.StartTime,
            endTime = s.EndTime,
            meetingPlace = s.MeetingPlace,
            status = s.Status
        }).ToList();
    }

    public async Task<List<object>> GetAvailableForStudentAsync(
        EmployeeId supervisorId,
        StudentApplicationId studentApplicationId)
    {
        // Бизнес-логика: только слоты по заявкам, где студент в статусе Собеседование
        var eligibleApplicationIds = await _studentSupervisorRepository
            .GetApplicationIdsByStudentAndStatusAsync(
                studentApplicationId,
                StudentSupervisorApplicationStatus.Interview);

        var slots = await _repository.GetAvailableForStudentAsync(
            supervisorId, eligibleApplicationIds);

        return slots.Select(s => (object)new
        {
            idInterviewSlot = s.IdInterviewSlot,
            startTime = s.StartTime,
            endTime = s.EndTime,
            meetingPlace = s.MeetingPlace
        }).ToList();
    }

    public async Task<object> ConfirmAsync(InterviewSlotId id)
    {
        var slot = await _repository.GetByIdAsync(id);
        if (slot == null)
            throw new KeyNotFoundException("Слот не найден");

        if (slot.Status != InterviewSlotStatus.SuggestedtoSupervisor)
            throw new InvalidOperationException(
                $"Нельзя подтвердить слот в статусе {slot.Status}");

        slot.Status = InterviewSlotStatus.Confirmed;
        await _repository.UpdateAsync(slot);

        return new
        {
            idInterviewSlot = slot.IdInterviewSlot,
            status = slot.Status,
            message = "Слот подтверждён"
        };
    }

    public async Task<object> RejectAsync(InterviewSlotId id, RejectSlotDto dto)
    {
        var slot = await _repository.GetByIdAsync(id);
        if (slot == null)
            throw new KeyNotFoundException("Слот не найден");

        if (slot.Status != InterviewSlotStatus.SuggestedtoSupervisor)
            throw new InvalidOperationException(
                $"Нельзя отклонить слот в статусе {slot.Status}");

        slot.Status = InterviewSlotStatus.Cancelled;
        slot.RejectionComment = dto.Comment;
        await _repository.UpdateAsync(slot);

        return new
        {
            idInterviewSlot = slot.IdInterviewSlot,
            status = slot.Status,
            rejectionComment = slot.RejectionComment,
            message = "Слот отклонён"
        };
    }

    public async Task<object> PublishAsync(
        InterviewSlotId id,
        SupervisorApplicationId? supervisorApplicationId)
    {
        var slot = await _repository.GetByIdAsync(id);
        if (slot == null)
            throw new KeyNotFoundException("Слот не найден");

        if (slot.Status != InterviewSlotStatus.Confirmed)
            throw new InvalidOperationException(
                "Публиковать можно только подтверждённые слоты");

        List<StudentApplicationId> eligibleStudents;
        string scope;

        if (supervisorApplicationId.HasValue)
        {
            // Бизнес-логика: проверяем конкретную заявку
            eligibleStudents = await _studentSupervisorRepository
                .GetStudentIdsByApplicationAndStatusAsync(
                    supervisorApplicationId.Value,
                    StudentSupervisorApplicationStatus.Interview);

            scope = $"по заявке {supervisorApplicationId.Value}";
        }
        else
        {
            // Бизнес-логика: все заявки руководителя
            var allApplicationIds = await _repository
                .GetAllApplicationIdsBySupervisorAsync(slot.IdEmployee);

            eligibleStudents = await _studentSupervisorRepository
                .GetDistinctStudentIdsByApplicationsAndStatusAsync(
                    allApplicationIds,
                    StudentSupervisorApplicationStatus.Interview);

            scope = "по всем заявкам руководителя";
        }

        if (!eligibleStudents.Any())
            throw new InvalidOperationException(
                $"Нет студентов со статусом Собеседование {scope}");

        slot.IdSupervisorApplication = supervisorApplicationId;
        slot.Status = InterviewSlotStatus.Published;
        await _repository.UpdateAsync(slot);

        return new
        {
            idInterviewSlot = slot.IdInterviewSlot,
            status = slot.Status,
            scope,
            studentsToNotify = eligibleStudents,
            studentsCount = eligibleStudents.Count,
            message = $"Слот опубликован для {eligibleStudents.Count} студентов {scope}"
        };
    }

    public async Task<object> BookAsync(InterviewSlotId id, [FromBody] BookSlotDto dto)
    {
        var slot = await _repository.GetByIdWithInterviewAsync(id);
        if (slot == null)
            throw new KeyNotFoundException("Слот не найден");

        if (slot.Status != InterviewSlotStatus.Published)
            throw new InvalidOperationException("Слот недоступен для бронирования");

        // Бизнес-логика: проверяем право студента на запись
        if (slot.IdSupervisorApplication.HasValue)
        {
            var isEligible = await _studentSupervisorRepository
                .IsStudentEligibleForSlotAsync(
                    slot.IdSupervisorApplication.Value,
                    dto.IdStudentApplication,
                    StudentSupervisorApplicationStatus.Interview);

            if (!isEligible)
                throw new InvalidOperationException(
                    "Студент не может записаться на этот слот");
        }

        slot.Status = InterviewSlotStatus.Booked;
        await _repository.UpdateAsync(slot);

        var interview = new Interview
        {
            IdInterviewSlot = id,
            IdStudentApplication = dto.IdStudentApplication,
            InterviewType = InterviewType.Supervisor,
            Status = InterviewStatus.Scheduled,
            Result = false
        };
        await _repository.AddInterviewAsync(interview);

        return new
        {
            idInterviewSlot = id,
            message = $"Собеседование назначено на " +
                      $"{slot.StartTime:dd MMMM} в " +
                      $"{slot.StartTime:HH:mm}"
        };
    }

    public async Task<object> CreateFromIntervalAsync(CreateTimeIntervalDto dto)
    {
        // Бизнес-логика: валидация времени
        if (dto.StartTime >= dto.EndTime)
            throw new ArgumentException(
                "Время окончания должно быть позже времени начала");

        var interval = new TimeInterval
        {
            IdEmployee = dto.IdEmployee,
            IdCreator = dto.IdCreator,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            MaxCount = dto.MaxCount,
            BreakDuration = dto.BreakDuration,
        };

        await _repository.AddIntervalAsync(interval);

        // Бизнес-логика: статус слотов зависит от того, кто создаёт
        bool createdBySupervisor = dto.IdCreator == null
                                || dto.IdCreator == dto.IdEmployee;

        var slotStatus = createdBySupervisor
            ? InterviewSlotStatus.Confirmed
            : InterviewSlotStatus.SuggestedtoSupervisor;

        var slots = GenerateSlots(interval, dto.MaxCount, dto.BreakDuration, slotStatus);
        await _repository.AddRangeAsync(slots);

        return new
        {
            idInterval = interval.IdInterval,
            slotsCreated = slots.Count,
            slotStatus = slotStatus,
            message = createdBySupervisor
                ? "Интервал создан, слоты подтверждены"
                : "Интервал создан, слоты отправлены на согласование руководителю"
        };
    }

    // === Вспомогательный метод генерации слотов ===
    private static List<InterviewSlot> GenerateSlots(
        TimeInterval interval,
        int count,
        TimeSpan? breakDuration,
        InterviewSlotStatus status)
    {
        var slots = new List<InterviewSlot>();
        var totalDuration = interval.EndTime - interval.StartTime;
        var breakTime = breakDuration ?? TimeSpan.Zero;
        var slotDuration = (totalDuration - breakTime * (count - 1)) / count;
        var currentStart = interval.StartTime;

        for (int i = 0; i < count; i++)
        {
            var slotEnd = currentStart + slotDuration;
            slots.Add(new InterviewSlot
            {
                IdEmployee = interval.IdEmployee,
                IdCreator = interval.IdCreator,
                IdInterval = interval.IdInterval,
                StartTime = currentStart,
                EndTime = slotEnd,
                Status = status,
            });
            currentStart = slotEnd + breakTime;
        }

        return slots;
    }
}
