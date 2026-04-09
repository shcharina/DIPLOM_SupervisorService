using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Services;

public class SupervisorApplicationStatusService
{
    private readonly AppDbContext _context;
    public SupervisorApplicationStatusService(AppDbContext context)
    {
        _context = context;
    }

    // Вызывается каждый раз когда меняется статус студента
    public async Task CheckAndUpdateApplicationStatus(SupervisorApplicationId supervisorApplicationId)
    {
        var application = await _context.SupervisorApplications
            .FirstOrDefaultAsync(a => 
                a.IdSupervisorApplication == supervisorApplicationId);

        if (application == null) return;

        // Проверяем только активные заявки
        if (application.Status != SupervisorApplicationStatus.Отправлена &&
            application.Status != SupervisorApplicationStatus.Удовлетворена) return;

        // Считаем принятых студентов + тех, кто еще в процессе оформления
        var acceptedCount = await _context.StudentSupervisorApplications
            .CountAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                (s.Status == StudentSupervisorApplicationStatus.ОформлениеДокументов ||
                 s.Status == StudentSupervisorApplicationStatus.Принят));

        // Если набрали нужное количество → Удовлетворена
        if (acceptedCount >= application.RequestedStudentsCount)
        {
            application.Status = SupervisorApplicationStatus.Удовлетворена;
            application.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
        else if (application.Status == SupervisorApplicationStatus.Удовлетворена)
        {
            // Студент отозвал заявку и количество стало меньше нужного
            // То ставим обратно статус Отправлена
            application.Status = SupervisorApplicationStatus.Отправлена;
            application.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

    }

}