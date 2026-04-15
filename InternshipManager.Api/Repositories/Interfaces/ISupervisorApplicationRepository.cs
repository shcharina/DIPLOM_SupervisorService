using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Repositories.Interfaces;

public interface ISupervisorApplicationRepository
{
    Task<SupervisorApplication?> GetByIdAsync(SupervisorApplicationId id);
    Task<(List<SupervisorApplication> Data, int TotalItems)> GetBySupervisorAsync(
        EmployeeId supervisorId, int page, int pageSize, 
        SupervisorApplicationStatus? status);
    Task<(List<SupervisorApplication> Data, int TotalItems)> GetActiveAsync(
        int page, int pageSize);
    Task<List<SupervisorApplication>> GetActivePracticesAsync(EmployeeId supervisorId);
    Task<List<SupervisorApplicationId>> GetCompletedApplicationIdsAsync(
        EmployeeId supervisorId);
    Task<List<SupervisorApplication>> GetExpiredSentApplicationsAsync();
    Task<List<SupervisorApplication>> GetCompletedSatisfiedApplicationsAsync();
    Task<SupervisorApplication> AddAsync(SupervisorApplication application);
    Task UpdateAsync(SupervisorApplication application);
    Task DeleteAsync(SupervisorApplication application);
}