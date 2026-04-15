using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;

namespace InternshipManager.Api.Repositories;

public class SupervisorApplicationRepository : ISupervisorApplicationRepository
{
    private readonly AppDbContext _context;
    public SupervisorApplicationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SupervisorApplication?> GetByIdAsync(SupervisorApplicationId id)
    {
        return await _context.SupervisorApplications
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdSupervisorApplication == id);
    }
    public async Task<(List<SupervisorApplication> Data, int TotalItems)> 
        GetBySupervisorAsync(
            EmployeeId supervisorId, int page, int pageSize,
            SupervisorApplicationStatus? status)
    {
        var query = _context.SupervisorApplications
            .AsNoTracking()
            .Where(a => a.IdEmployee == supervisorId);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        var totalItems = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.IdSupervisorApplication)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalItems);
    }

    public async Task<(List<SupervisorApplication> Data, int TotalItems)> 
        GetActiveAsync(int page, int pageSize)
    {
        var query = _context.SupervisorApplications
            .AsNoTracking()
            .Where(a => a.Status == SupervisorApplicationStatus.Sent);

        var totalItems = await query.CountAsync();
        var data = await query
            .OrderByDescending(a => a.IdSupervisorApplication)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (data, totalItems);
    }

    public async Task<List<SupervisorApplication>> GetActivePracticesAsync(
        EmployeeId supervisorId)
    {
        var today = DateTime.UtcNow;
        var applicationIds = await _context.SupervisorApplications
            .AsNoTracking()
            .Where(a =>
                a.IdEmployee == supervisorId &&
                a.StartDate != null && a.StartDate <= today &&
                a.EndDate != null && a.EndDate >= today)
            .Select(a => a.IdSupervisorApplication)
            .ToListAsync();

        var idsWithStudents = await _context.StudentSupervisorApplications
            .AsNoTracking()
            .Where(s =>
                applicationIds.Contains(s.IdSupervisorApplication) &&
                (s.Status == StudentSupervisorApplicationStatus.Accepted ||
                 s.Status == StudentSupervisorApplicationStatus.DocumentProcessing))
            .Select(s => s.IdSupervisorApplication)
            .Distinct()
            .ToListAsync();

        return await _context.SupervisorApplications
            .AsNoTracking()
            .Where(a => idsWithStudents.Contains(a.IdSupervisorApplication))
            .ToListAsync();

    }

    // Для Add/Update/Delete — без AsNoTracking, т.к. нужно отслеживать

    public async Task<SupervisorApplication> AddAsync(SupervisorApplication application)
    {
        _context.SupervisorApplications.Add(application);
        await _context.SaveChangesAsync();
        return application;
    }

    public async Task UpdateAsync(SupervisorApplication application)
    {
        _context.SupervisorApplications.Update(application);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(SupervisorApplication application)
    {
        _context.SupervisorApplications.Remove(application);
        await _context.SaveChangesAsync();
    }
}