using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Services;

public class SupervisorApplicationDeadlineCheckerService : BackgroundService
{
    // Нужен IServiceScopeFactory потому что DbContext не thread-safe
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
        // Проверяем каждые 24 часа
        while (!stoppingToken.IsCancellationRequested)
        {
            await CheckExpiredApplications();

            // Ждём до следующей проверки
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }

    }

    private async Task CheckExpiredApplications()
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        _logger.LogInformation("Проверка заявок, у которых наступила дата начала практики: {time}", DateTime.UtcNow);

        // Находим все отправленные заявки у которых дата начала уже наступила
        var startedApplications = await context.SupervisorApplications
            .Where(a =>
                a.Status == SupervisorApplicationStatus.Отправлена &&
                a.StartDate != null &&
                a.StartDate <= DateTime.UtcNow)
            .ToListAsync();

        _logger.LogInformation(
            "Найдено заявок, у которых наступила дата начала практики: {count}", startedApplications.Count);

        foreach (var application in startedApplications)
        {
            // Считаем принятых студентов
            var acceptedCount = await context.StudentSupervisorApplications
                .CountAsync(s =>
                    s.IdSupervisorApplication == application.IdSupervisorApplication &&
                    (s.Status == StudentSupervisorApplicationStatus.ОформлениеДокументов ||
                     s.Status == StudentSupervisorApplicationStatus.Принят));

            if (acceptedCount >= application.RequestedStudentsCount)
            {
                // Набрали нужное количество → Удовлетворена
                application.Status = SupervisorApplicationStatus.Удовлетворена;

                _logger.LogInformation(
                    "Заявка {id} → Удовлетворена ({count}/{required} студентов)",
                    application.IdSupervisorApplication,
                    acceptedCount,
                    application.RequestedStudentsCount);
            }
            else
            {
                // Дата наступила, но студентов не хватает → Неудовлетворена
                application.Status = SupervisorApplicationStatus.Неудовлетворена;

                _logger.LogInformation(
                    "Заявка {id} → Неудовлетворена ({count}/{required} студентов)",
                    application.IdSupervisorApplication,
                    acceptedCount,
                    application.RequestedStudentsCount);
            }
        }

        await context.SaveChangesAsync();

        // Проверка на окончание практики - необходимость отзыва
        await CheckPendingReviews(context);
    }

    private async Task CheckPendingReviews(AppDbContext context)
    {
        _logger.LogInformation(
            "Проверка необходимости отзывов: {time}",
            DateTime.UtcNow);
        
        var completedApplications = await context.SupervisorApplications
            .Where(a =>
            a.EndDate != null &&
            a.EndDate <= DateTime.UtcNow &&
            a.Status == SupervisorApplicationStatus.Удовлетворена)
            .ToListAsync();

        foreach (var application in completedApplications)
        {
            var completedStudents = await context.StudentSupervisorApplications
                .Where(s =>
                    s.IdSupervisorApplication == application.IdSupervisorApplication &&
                    s.Status == StudentSupervisorApplicationStatus.Принят)
                .ToArrayAsync();
            
            foreach (var student in completedStudents)
            {
                var reviewExists = await context.SupervisorReviews
                    .AnyAsync(r =>
                    r.IdEmployee == application.IdEmployee &&
                    r.IdStudentApplication == student.IdStudentApplication);
                
                if (!reviewExists)
                {
                    _logger.LogInformation(
                        "Руководитель {supervisor} должен оставить отзыв о студенте {student}",
                        application.IdEmployee,
                        student.IdStudentApplication);

                    // тут потом добавить какую-нибудь функцию типа уведомление руководителю    
                }
            }
        }
    }
}