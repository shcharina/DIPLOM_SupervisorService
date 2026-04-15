using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;

namespace InternshipManager.Api.Repositories;

public class StudentSupervisorApplicationRepository
    : IStudentSupervisorApplicationRepository
{
    private readonly AppDbContext _context;

    public StudentSupervisorApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    // --- Методы для InterviewSlotService ---

    // Для GetAvailable: какие заявки руководителя доступны студенту
    // Источник: InterviewSlotController.GetAvailable →
    //   _context.StudentSupervisorApplications
    //     .Where(s => s.IdStudentApplication == studentApplicationId
    //              && s.Status == Собеседование)
    //     .Select(s => s.IdSupervisorApplication)
    public async Task<List<SupervisorApplicationId>>
        GetApplicationIdsByStudentAndStatusAsync(
            StudentApplicationId studentApplicationId,
            StudentSupervisorApplicationStatus status)
    {
        return await _context.StudentSupervisorApplications
            .AsNoTracking()
            .Where(s =>
                s.IdStudentApplication == studentApplicationId &&
                s.Status == status)
            .Select(s => s.IdSupervisorApplication)
            .ToListAsync();
    }

    // Для Publish по одной заявке: студенты конкретной заявки с нужным статусом
    // Источник: InterviewSlotController.Publish (ветка supervisorApplicationId.HasValue) →
    //   _context.StudentSupervisorApplications
    //     .Where(s => s.IdSupervisorApplication == supervisorApplicationId
    //              && s.Status == Собеседование)
    //     .Select(s => s.IdStudentApplication)
    public async Task<List<StudentApplicationId>>
        GetStudentIdsByApplicationAndStatusAsync(
            SupervisorApplicationId supervisorApplicationId,
            StudentSupervisorApplicationStatus status)
    {
        return await _context.StudentSupervisorApplications
            .AsNoTracking()
            .Where(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.Status == status)
            .Select(s => s.IdStudentApplication)
            .ToListAsync();
    }

    // Для Publish по всем заявкам: уникальные студенты по списку заявок
    // Источник: InterviewSlotController.Publish (ветка else) →
    //   _context.StudentSupervisorApplications
    //     .Where(s => allApplicationIds.Contains(s.IdSupervisorApplication)
    //              && s.Status == Собеседование)
    //     .Select(s => s.IdStudentApplication)
    //     .Distinct()
    public async Task<List<StudentApplicationId>>
        GetDistinctStudentIdsByApplicationsAndStatusAsync(
            List<SupervisorApplicationId> supervisorApplicationIds,
            StudentSupervisorApplicationStatus status)
    {
        return await _context.StudentSupervisorApplications
            .AsNoTracking()
            .Where(s =>
                supervisorApplicationIds.Contains(s.IdSupervisorApplication) &&
                s.Status == status)
            .Select(s => s.IdStudentApplication)
            .Distinct()
            .ToListAsync();
    }

    // Для Book: проверка права студента на запись к слоту
    // Источник: InterviewSlotController.Book →
    //   _context.StudentSupervisorApplications
    //     .AnyAsync(s => s.IdSupervisorApplication == slot.IdSupervisorApplication
    //               && s.IdStudentApplication == dto.IdStudentApplication
    //               && s.Status == Собеседование)
    public async Task<bool> IsStudentEligibleForSlotAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId,
        StudentSupervisorApplicationStatus status)
    {
        return await _context.StudentSupervisorApplications
            .AsNoTracking()
            .AnyAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId &&
                s.Status == status);
    }

    // --- Существующие методы из StudentSupervisorApplicationController ---

    // Для GetStudents: список студентов по заявке с опциональным фильтром статуса
    public async Task<List<StudentSupervisorApplication>> GetByApplicationAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentSupervisorApplicationStatus? status)
    {
        var query = _context.StudentSupervisorApplications
            .AsNoTracking()
            .Where(s => s.IdSupervisorApplication == supervisorApplicationId);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        return await query.ToListAsync();
    }

    // Для GetByStudent: все заявки конкретного студента
    public async Task<List<StudentSupervisorApplication>> GetByStudentAsync(
        StudentApplicationId studentApplicationId)
    {
        return await _context.StudentSupervisorApplications
            .AsNoTracking()
            .Where(s => s.IdStudentApplication == studentApplicationId)
            .ToListAsync();
    }

    // Для AcceptWithoutInterview / InviteToInterview / Reject:
    // получить конкретную связку студент-заявка
    public async Task<StudentSupervisorApplication?> GetLinkAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        return await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);
    }

    // Для AcceptWithoutInterview: подсчёт уже принятых студентов
    // Источник: _context.StudentSupervisorApplications
    //   .CountAsync(s => s.IdSupervisorApplication == supervisorApplicationId
    //               && (s.Status == DocumentProcessing || s.Status == Accepted))
    public async Task<int> CountAcceptedAsync(
        SupervisorApplicationId supervisorApplicationId)
    {
        return await _context.StudentSupervisorApplications
            .AsNoTracking()
            .CountAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                (s.Status == StudentSupervisorApplicationStatus.DocumentProcessing ||
                 s.Status == StudentSupervisorApplicationStatus.Accepted));
    }

    // Для AssignStudent: проверка дубликата
    public async Task<bool> ExistsAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        return await _context.StudentSupervisorApplications
            .AsNoTracking()
            .AnyAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);
    }

    // Для AssignStudent: добавить связку
    public async Task AddAsync(StudentSupervisorApplication link)
    {
        _context.StudentSupervisorApplications.Add(link);
        await _context.SaveChangesAsync();
    }

    // Для AcceptWithoutInterview / InviteToInterview / Reject: обновить статус
    public async Task UpdateAsync(StudentSupervisorApplication link)
    {
        _context.StudentSupervisorApplications.Update(link);
        await _context.SaveChangesAsync();
    }
}