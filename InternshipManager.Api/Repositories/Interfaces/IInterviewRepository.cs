using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Repositories.Interfaces;

public interface IInterviewRepository
{
    Task<object?> GetBySupervisorAsync(EmployeeId supervisorId);
    Task<object?> GetByIdAsync(InterviewSlotId id);
    Task<Interview?> FindAsync(InterviewSlotId id);
    Task<InterviewSlot?> FindSlotAsync(InterviewSlotId id);
    Task<StudentSupervisorApplication?> FindLinkAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId);
    Task SaveChangesAsync();
}
