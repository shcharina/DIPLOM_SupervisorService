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
        _logger.LogInformation("Проверка просроченных заявок: {time}", DateTime.UtcNow);
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Находим все отправленные заявки у которых дата начала уже наступила
        var expiredApplications = await context.SupervisorApplications
            .Where(a =>
                a.Status == SupervisorApplicationStatus.Отправлена &&
                a.StartDate != null &&
                a.StartDate <= DateTime.UtcNow)
            .ToListAsync();

        _logger.LogInformation(
            "Найдено просроченных заявок: {count}", expiredApplications.Count);

        foreach (var application in expiredApplications)
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

            application.UpdatedAt = DateTime.UtcNow;

        }

        await context.SaveChangesAsync();

    }

}