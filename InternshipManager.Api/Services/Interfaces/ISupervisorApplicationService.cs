using InternshipManager.Api.DTOs.SupervisorApplication;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Services.Interfaces;

public interface ISupervisorApplicationService
{
    Task<SupervisorApplication?> GetByIdAsync(SupervisorApplicationId id);
    Task<(List<SupervisorApplication> Data, int TotalItems)> GetBySupervisorAsync(
        EmployeeId supervisorId, int page, int pageSize,
        SupervisorApplicationStatus? status);
    Task<(List<SupervisorApplication> Data, int TotalItems)> GetActiveAsync(
        int page, int pageSize);
    Task<List<SupervisorApplication>> GetActivePracticesAsync(EmployeeId supervisorId);
    Task<SupervisorApplicationResponseDto> CreateAsync(CreateSupervisorApplicationDto dto);
    Task<SupervisorApplicationResponseDto> UpdateAsync(
        SupervisorApplicationId id, UpdateSupervisorApplicationDto dto);

    Task DeleteAsync(SupervisorApplicationId id);
    Task<object> SendAsync(SupervisorApplicationId id);
    Task<object> CancelAsync(SupervisorApplicationId id);
    Task<object> CloseAsync(SupervisorApplicationId id);
}