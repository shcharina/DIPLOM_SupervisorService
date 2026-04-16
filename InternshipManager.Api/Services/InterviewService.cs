using InternshipManager.Api.DTOs.Interview;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Repositories.Interfaces;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Services;

public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _repository;
    private readonly ISupervisorApplicationStatusService _statusService;

    public InterviewService(
        IInterviewRepository repository,
        ISupervisorApplicationStatusService statusService)
    {
        _repository    = repository;
        _statusService = statusService;
    }

    public async Task<object> GetBySupervisorAsync(EmployeeId supervisorId)
    {
        return await _repository.GetBySupervisorAsync(supervisorId) ?? new List<object>();
    }

    public async Task<object?> GetByIdAsync(InterviewSlotId id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<object> RecordResultAsync(
        InterviewSlotId id, RecordInterviewResultDto dto)
    {
        var interview = await _repository.FindAsync(id);
        if (interview == null)
            throw new KeyNotFoundException("Собеседование не найдено");

        interview.Result  = dto.Result;
        interview.Comment = dto.Comment;
        interview.Status  = InterviewStatus.IsOver;

        var slot = await _repository.FindSlotAsync(id);

        if (slot?.IdSupervisorApplication != null)
        {
            var link = await _repository.FindLinkAsync(
                slot.IdSupervisorApplication.Value,
                interview.IdStudentApplication);

            if (link != null)
            {
                if (link.Status == StudentSupervisorApplicationStatus.Accepted)
                    throw new InvalidOperationException(
                        "Студент уже принят, повторное редактирование невозможно");

                link.Status = dto.Result
                    ? StudentSupervisorApplicationStatus.DocumentProcessing
                    : StudentSupervisorApplicationStatus.Rejected;
            }
        }

        // === Один save — сохраняет и interview, и link ===
        await _repository.SaveChangesAsync();

        // === Проверка статуса ПОСЛЕ сохранения — видит актуальные данные ===
        if (slot?.IdSupervisorApplication != null && dto.Result)
        {
            await _statusService.CheckAndUpdateApplicationStatus(
                slot.IdSupervisorApplication.Value);
        }

        return new
        {
            idInterviewSlot  = id,
            result           = interview.Result,
            newStudentStatus = dto.Result
                ? StudentSupervisorApplicationStatus.DocumentProcessing
                : StudentSupervisorApplicationStatus.Rejected,
            message = dto.Result
                ? "Собеседование пройдено, студент переведён в оформление документов"
                : "Собеседование не пройдено, студент отклонён"
        };
    }
}