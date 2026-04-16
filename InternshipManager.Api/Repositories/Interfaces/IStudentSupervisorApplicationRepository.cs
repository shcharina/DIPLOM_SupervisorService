using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Repositories.Interfaces;

public interface IStudentSupervisorApplicationRepository
{
    // Для GetAvailable в InterviewSlotService
    // Возвращает ID заявок руководителя, где студент в нужном статусе
    Task<List<SupervisorApplicationId>> GetApplicationIdsByStudentAndStatusAsync(
        StudentApplicationId studentApplicationId,
        StudentSupervisorApplicationStatus status);

    // Для Publish по одной заявке
    // Возвращает ID студентов по конкретной заявке руководителя с нужным статусом
    Task<List<StudentApplicationId>> GetStudentIdsByApplicationAndStatusAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentSupervisorApplicationStatus status);

    // Для Publish по всем заявкам руководителя
    // Возвращает уникальные ID студентов по списку заявок с нужным статусом
    Task<List<StudentApplicationId>> GetDistinctStudentIdsByApplicationsAndStatusAsync(
        List<SupervisorApplicationId> supervisorApplicationIds,
        StudentSupervisorApplicationStatus status);

    // Для Book
    // Проверяет, может ли студент записаться на слот (есть ли связка с нужным статусом)
    Task<bool> IsStudentEligibleForSlotAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId,
        StudentSupervisorApplicationStatus status);

    // Существующие методы из StudentSupervisorApplicationController
    Task<List<StudentSupervisorApplication>> GetByApplicationAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentSupervisorApplicationStatus? status);

    Task<List<StudentSupervisorApplication>> GetActiveByApplicationTrackedAsync(
        SupervisorApplicationId supervisorApplicationId);

    Task<List<StudentSupervisorApplication>> GetByStudentAsync(
        StudentApplicationId studentApplicationId);

    Task<StudentSupervisorApplication?> GetLinkAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId);

    Task<List<StudentSupervisorApplication>> GetByApplicationIdsAndStatusAsync(
        List<SupervisorApplicationId> applicationIds,
        StudentSupervisorApplicationStatus status);

    Task<bool> ExistsByStudentAndStatusAsync(
        StudentApplicationId studentApplicationId,
        StudentSupervisorApplicationStatus status);

    Task<int> CountAcceptedAsync(SupervisorApplicationId supervisorApplicationId);

    Task<bool> ExistsAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId);

    Task AddAsync(StudentSupervisorApplication link);
    Task UpdateAsync(StudentSupervisorApplication link);
}