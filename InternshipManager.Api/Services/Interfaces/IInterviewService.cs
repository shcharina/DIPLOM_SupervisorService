using InternshipManager.Api.DTOs.Interview;

namespace InternshipManager.Api.Services.Interfaces;

public interface IInterviewService
{
    Task<object>  GetBySupervisorAsync(EmployeeId supervisorId);
    Task<object?> GetByIdAsync(InterviewSlotId id);
    Task<object>  RecordResultAsync(InterviewSlotId id, RecordInterviewResultDto dto);
}