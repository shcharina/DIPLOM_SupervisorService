using InternshipManager.Api.Enums;
using InternshipManager.Api.Repositories.Interfaces;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Services;

public class SupervisorApplicationStatusService 
    : ISupervisorApplicationStatusService
{
    private readonly ISupervisorApplicationRepository _applicationRepository;
    private readonly IStudentSupervisorApplicationRepository _studentRepository;

    public SupervisorApplicationStatusService(
        ISupervisorApplicationRepository applicationRepository,
        IStudentSupervisorApplicationRepository studentRepository)
    {
        _applicationRepository = applicationRepository;
        _studentRepository = studentRepository;
    }

    public async Task CheckAndUpdateApplicationStatus(
        SupervisorApplicationId supervisorApplicationId)
    {
        var application = await _applicationRepository
            .GetByIdAsync(supervisorApplicationId);

        if (application == null) return;

        // Проверяем только заявки в релевантных статусах
        if (application.Status != SupervisorApplicationStatus.Sent &&
            application.Status != SupervisorApplicationStatus.Satisfied)
            return;

        // CountAcceptedAsync уже есть в репозитории —
        // считает студентов со статусом DocumentProcessing или Accepted
        var acceptedCount = await _studentRepository
            .CountAcceptedAsync(supervisorApplicationId);

        if (acceptedCount >= application.RequestedStudentsCount)
        {
            application.Status = SupervisorApplicationStatus.Satisfied;
            await _applicationRepository.UpdateAsync(application);
        }
        else if (application.Status == SupervisorApplicationStatus.Satisfied)
        {
            // Если кто-то отвалился — откатываем статус обратно
            application.Status = SupervisorApplicationStatus.Sent;
            await _applicationRepository.UpdateAsync(application);
        }
    }
}