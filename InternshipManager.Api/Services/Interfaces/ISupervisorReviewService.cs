using InternshipManager.Api.DTOs.SupervisorReview;

namespace InternshipManager.Api.Services.Interfaces;

public interface ISupervisorReviewService
{
    Task<object> GetPendingReviewsAsync(EmployeeId supervisorId);
    Task<object> CreateAsync(CreateSupervisorReviewDto dto);
    Task<object?> GetReviewAsync(
        EmployeeId idEmployee,
        StudentApplicationId idStudentApplication);
}