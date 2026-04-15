using InternshipManager.Api.DTOs.StudentSupervisorApplication;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Services.Interfaces;

public interface IStudentSupervisorApplicationService
{
    Task<List<object>> GetStudentsAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentSupervisorApplicationStatus? status);

    Task<object> GetByStudentAsync(StudentApplicationId studentApplicationId);

    Task<object> GetStudentDetailsAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId);

    Task<object> AssignStudentAsync(AssignStudentDto dto);

    Task<object> AcceptWithoutInterviewAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId);

    Task<object> InviteToInterviewAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId);

    Task<object> RejectAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId);
}