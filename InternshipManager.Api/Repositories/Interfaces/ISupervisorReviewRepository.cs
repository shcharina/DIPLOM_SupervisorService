using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Repositories.Interfaces;

public interface ISupervisorReviewRepository
{
    Task<SupervisorReview?> GetAsync(
        EmployeeId idEmployee,
        StudentApplicationId idStudentApplication);

    Task<bool> ExistsAsync(
        EmployeeId idEmployee,
        StudentApplicationId idStudentApplication);

    Task<List<StudentApplicationId>> GetReviewedStudentIdsAsync(
        EmployeeId supervisorId);

    Task AddAsync(SupervisorReview review);
}