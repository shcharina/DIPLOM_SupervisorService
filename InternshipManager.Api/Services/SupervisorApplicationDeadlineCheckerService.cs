using InternshipManager.Api.Enums;
using InternshipManager.Api.Repositories.Interfaces;

namespace InternshipManager.Api.Services;

public class SupervisorApplicationDeadlineCheckerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SupervisorApplicationDeadlineCheckerService> _logger;

    public SupervisorApplicationDeadlineCheckerService(
        IServiceScopeFactory scopeFactory,
        ILogger<SupervisorApplicationDeadlineCheckerService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Фоновая задача проверки дат запущена");

        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiredApplications();
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }

    private async Task CheckExpiredApplications()
    {
        using var scope = _scopeFactory.CreateScope();

        var applicationRepo = scope.ServiceProvider
            .GetRequiredService<ISupervisorApplicationRepository>();
        var studentRepo = scope.ServiceProvider
            .GetRequiredService<IStudentSupervisorApplicationRepository>();
        var reviewRepo = scope.ServiceProvider
            .GetRequiredService<ISupervisorReviewRepository>();

        _logger.LogInformation(
            "Проверка заявок, у которых наступила дата начала практики: {time}",
            DateTime.UtcNow);

        // === Заявки, где практика уже началась, но статус ещё Sent ===
        var startedApplications = await applicationRepo
            .GetExpiredSentApplicationsAsync();

        _logger.LogInformation(
            "Найдено заявок, у которых наступила дата начала практики: {count}",
            startedApplications.Count);

        foreach (var application in startedApplications)
        {
            var acceptedCount = await studentRepo
                .CountAcceptedAsync(application.IdSupervisorApplication);

            if (acceptedCount >= application.RequestedStudentsCount)
            {
                application.Status = SupervisorApplicationStatus.Satisfied;
                _logger.LogInformation(
                    "Заявка {id} -> Удовлетворена ({count}/{required} студентов)",
                    application.IdSupervisorApplication,
                    acceptedCount,
                    application.RequestedStudentsCount);
            }
            else
            {
                application.Status = SupervisorApplicationStatus.Unsatisfied;
                _logger.LogInformation(
                    "Заявка {id} -> Неудовлетворена ({count}/{required} студентов)",
                    application.IdSupervisorApplication,
                    acceptedCount,
                    application.RequestedStudentsCount);
            }

            await applicationRepo.UpdateAsync(application);
        }

        // === Проверка незаполненных отзывов ===
        await CheckPendingReviews(applicationRepo, studentRepo, reviewRepo);
    }

    private async Task CheckPendingReviews(
        ISupervisorApplicationRepository applicationRepo,
        IStudentSupervisorApplicationRepository studentRepo,
        ISupervisorReviewRepository reviewRepo)
    {
        _logger.LogInformation(
            "Проверка незаполненных отзывов: {time}",
            DateTime.UtcNow);

        var completedApplications = await applicationRepo
            .GetCompletedSatisfiedApplicationsAsync();

        foreach (var application in completedApplications)
        {
            // Студенты со статусом Accepted по данной заявке
            var completedStudents = await studentRepo
                .GetByApplicationAsync(
                    application.IdSupervisorApplication,
                    StudentSupervisorApplicationStatus.Accepted);

            foreach (var student in completedStudents)
            {
                var reviewExists = await reviewRepo.ExistsAsync(
                    application.IdEmployee,
                    student.IdStudentApplication);

                if (!reviewExists)
                {
                    _logger.LogInformation(
                        "Руководитель {supervisor} должен оставить отзыв о студенте {student}",
                        application.IdEmployee,
                        student.IdStudentApplication);
                }
            }
        }
    }
}