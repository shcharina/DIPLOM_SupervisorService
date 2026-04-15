using InternshipManager.Api.DTOs.InterviewSlot;
using InternshipManager.Api.DTOs.TimeInterval;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Services.Interfaces;

public interface IInterviewSlotService
{
    Task<List<object>> GetPendingBySupervisorAsync(EmployeeId supervisorId);
    Task<List<object>> GetBySupervisorAsync(EmployeeId supervisorId);
    Task<List<object>> GetAvailableForStudentAsync(
        EmployeeId supervisorId,
        StudentApplicationId studentApplicationId);
    Task<object> ConfirmAsync(InterviewSlotId id);
    Task<object> RejectAsync(InterviewSlotId id, RejectSlotDto dto);
    Task<object> PublishAsync(InterviewSlotId id, SupervisorApplicationId? supervisorApplicationId);
    Task<object> BookAsync(InterviewSlotId id, BookSlotDto dto);
    Task<object> CreateFromIntervalAsync(CreateTimeIntervalDto dto);
}
