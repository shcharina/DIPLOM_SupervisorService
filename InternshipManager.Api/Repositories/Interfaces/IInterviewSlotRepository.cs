using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Repositories.Interfaces;

public interface IInterviewSlotRepository
{
    Task<InterviewSlot?> GetByIdAsync(InterviewSlotId id);
    Task<InterviewSlot?> GetByIdWithInterviewAsync(InterviewSlotId id);
    Task<List<InterviewSlot>> GetPendingBySupervisorAsync(EmployeeId supervisorId);
    Task<List<InterviewSlot>> GetBySupervisorAsync(EmployeeId supervisorId);
    Task<List<InterviewSlot>> GetAvailableForStudentAsync(
        EmployeeId supervisorId,
        List<SupervisorApplicationId> eligibleApplicationIds);
    Task<List<SupervisorApplicationId>> GetAllApplicationIdsBySupervisorAsync(
        EmployeeId supervisorId);
    Task UpdateAsync(InterviewSlot slot);
    Task AddInterviewAsync(Interview interview);
    Task AddRangeAsync(List<InterviewSlot> slots);
    Task AddIntervalAsync(TimeInterval interval);
}
