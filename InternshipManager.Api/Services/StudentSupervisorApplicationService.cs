using InternshipManager.Api.DTOs.StudentSupervisorApplication;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Services;

public class StudentSupervisorApplicationService : IStudentSupervisorApplicationService
{
    private readonly IStudentSupervisorApplicationRepository _repository;
    private readonly ISupervisorApplicationRepository _supervisorAppRepository;
    private readonly SupervisorApplicationStatusService _statusService;
    private readonly ManagerApiClient _managerApi;
    private readonly StudentApiClient _studentApi;

    public StudentSupervisorApplicationService(
        IStudentSupervisorApplicationRepository repository,
        ISupervisorApplicationRepository supervisorAppRepository,
        SupervisorApplicationStatusService statusService,
        ManagerApiClient managerApi,
        StudentApiClient studentApi)
    {
        _repository = repository;
        _supervisorAppRepository = supervisorAppRepository;
        _statusService = statusService;
        _managerApi = managerApi;
        _studentApi = studentApi;
    }

    public async Task<List<object>> GetStudentsAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentSupervisorApplicationStatus? status)
    {
        var students = await _repository
            .GetByApplicationAsync(supervisorApplicationId, status);

        return students.Select(s => (object)new
        {
            idStudentApplication = s.IdStudentApplication,
            status = s.Status
        }).ToList();
    }

    public async Task<object> GetByStudentAsync(
        StudentApplicationId studentApplicationId)
    {
        var links = await _repository.GetByStudentAsync(studentApplicationId);

        return links.Select(s => new
        {
            idSupervisorApplication = s.IdSupervisorApplication,
            idStudentApplication = s.IdStudentApplication,
            status = s.Status,
            statusName = s.Status.ToString()
        }).ToList();
    }

    public async Task<object> GetStudentDetailsAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        var link = await _repository.GetLinkAsync(
            supervisorApplicationId, studentApplicationId);

        if (link == null)
            throw new KeyNotFoundException("Студент не найден в этой заявке");

        // Параллельные запросы к внешним сервисам — бизнес-логика сборки данных
        var studentApplicationTask = _studentApi
            .GetStudentApplicationAsync(studentApplicationId);
        var questionnaireTask = _studentApi
            .GetQuestionnaireAsync(studentApplicationId);
        var testingResultTask = _managerApi
            .GetTestingResultAsync(studentApplicationId);
        var managerInterviewTask = _managerApi
            .GetManagerInterviewResultAsync(studentApplicationId);

        await Task.WhenAll(
            studentApplicationTask,
            questionnaireTask,
            testingResultTask,
            managerInterviewTask);

        return new
        {
            studentApplication = studentApplicationTask.Result,
            questionnaire = questionnaireTask.Result,
            testingResult = testingResultTask.Result,
            managerInterviewResult = managerInterviewTask.Result,
            currentStatus = new
            {
                status = (int)link.Status,
                statusName = link.Status.ToString()
            }
        };
    }

    public async Task<object> AssignStudentAsync(AssignStudentDto dto)
    {
        // Бизнес-логика: заявка должна быть активной
        var supervisorApp = await _supervisorAppRepository
            .GetByIdAsync(dto.IdSupervisorApplication);

        if (supervisorApp == null ||
            supervisorApp.Status != SupervisorApplicationStatus.Отправлена)
            throw new KeyNotFoundException("Активная заявка руководителя не найдена");

        // Бизнес-логика: нельзя прикрепить студента дважды
        var exists = await _repository.ExistsAsync(
            dto.IdSupervisorApplication, dto.IdStudentApplication);

        if (exists)
            throw new InvalidOperationException(
                "Студент уже прикреплён к этой заявке");

        var link = new StudentSupervisorApplication
        {
            IdSupervisorApplication = dto.IdSupervisorApplication,
            IdStudentApplication = dto.IdStudentApplication,
            Status = StudentSupervisorApplicationStatus.НаРассмотренииРуководителем
        };

        await _repository.AddAsync(link);

        return new
        {
            idSupervisorApplication = link.IdSupervisorApplication,
            idStudentApplication = link.IdStudentApplication,
            status = (int)link.Status
        };
    }

    public async Task<object> AcceptWithoutInterviewAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        var link = await _repository.GetLinkAsync(
            supervisorApplicationId, studentApplicationId);

        if (link == null)
            throw new KeyNotFoundException("Связка студент-заявка не найдена");

        // Бизнес-логика: студент не должен быть отказавшим
        if (link.Status == StudentSupervisorApplicationStatus.Отказано)
            throw new InvalidOperationException(
                "Студент отозвал заявку, дальнейшая работа с ним невозможна");

        if (link.Status != StudentSupervisorApplicationStatus.НаРассмотренииРуководителем)
            throw new InvalidOperationException(
                $"Нельзя принять студента в статусе {link.Status}");

        // Бизнес-логика: проверка лимита студентов
        var currentAccepted = await _repository.CountAcceptedAsync(supervisorApplicationId);
        var application = await _supervisorAppRepository.GetByIdAsync(supervisorApplicationId);

        if (application != null &&
            currentAccepted >= application.RequestedStudentsCount)
            throw new InvalidOperationException(
                "Заявка уже набрала необходимое количество студентов");

        link.Status = StudentSupervisorApplicationStatus.ОформлениеДокументов;
        await _repository.UpdateAsync(link);

        await _statusService.CheckAndUpdateApplicationStatus(supervisorApplicationId);

        return new
        {
            idSupervisorApplication = supervisorApplicationId,
            idStudentApplication = studentApplicationId,
            status = link.Status,
            message = "Студент принят на практику, переходит к оформлению документов"
        };
    }

    public async Task<object> InviteToInterviewAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        var link = await _repository.GetLinkAsync(
            supervisorApplicationId, studentApplicationId);

        if (link == null)
            throw new KeyNotFoundException("Связка студент-заявка не найдена");

        if (link.Status == StudentSupervisorApplicationStatus.Отказано)
            throw new InvalidOperationException(
                "Студент отозвал заявку, дальнейшая работа с ним невозможна");

        if (link.Status != StudentSupervisorApplicationStatus.НаРассмотренииРуководителем)
            throw new InvalidOperationException(
                $"Нельзя пригласить студента в статусе {link.Status}");

        link.Status = StudentSupervisorApplicationStatus.Собеседование;
        await _repository.UpdateAsync(link);

        return new
        {
            idSupervisorApplication = supervisorApplicationId,
            idStudentApplication = studentApplicationId,
            status = link.Status,
            message = "Студент приглашён на собеседование"
        };
    }

    public async Task<object> RejectAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        var link = await _repository.GetLinkAsync(
            supervisorApplicationId, studentApplicationId);

        if (link == null)
            throw new KeyNotFoundException("Связка студент-заявка не найдена");

        link.Status = StudentSupervisorApplicationStatus.Отказано;
        await _repository.UpdateAsync(link);

        return new
        {
            idSupervisorApplication = supervisorApplicationId,
            idStudentApplication = studentApplicationId,
            status = link.Status,
            message = "Студенту отказано"
        };
    }
}