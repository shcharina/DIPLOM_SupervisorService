using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;

namespace InternshipManager.Api.Repositories;

public class InterviewSlotRepository : IInterviewSlotRepository
{
    private readonly AppDbContext _context;

    public InterviewSlotRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InterviewSlot?> GetByIdAsync(InterviewSlotId id)
    {
        return await _context.InterviewSlots
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdInterviewSlot == id);
    }

    public async Task<InterviewSlot?> GetByIdWithInterviewAsync(InterviewSlotId id)
    {
        return await _context.InterviewSlots
            .Include(s => s.Interview)
            .FirstOrDefaultAsync(s => s.IdInterviewSlot == id);
    }

    public async Task<List<InterviewSlot>> GetPendingBySupervisorAsync(EmployeeId supervisorId)
    {
        return await _context.InterviewSlots
            .AsNoTracking()
            .Where(s => s.IdEmployee == supervisorId
                     && s.Status == InterviewSlotStatus.SuggestedtoSupervisor)
            .ToListAsync();
    }

    public async Task<List<InterviewSlot>> GetBySupervisorAsync(EmployeeId supervisorId)
    {
        return await _context.InterviewSlots
            .AsNoTracking()
            .Where(s => s.IdEmployee == supervisorId
                     && s.Status != InterviewSlotStatus.Cancelled)
            .OrderByDescending(s => s.StartTime)
            .ToListAsync();
    }

    public async Task<List<InterviewSlot>> GetAvailableForStudentAsync(
        EmployeeId supervisorId,
        List<SupervisorApplicationId> eligibleApplicationIds)
    {
        return await _context.InterviewSlots
            .AsNoTracking()
            .Where(s =>
                s.IdEmployee == supervisorId &&
                s.Status == InterviewSlotStatus.Published &&
                s.IdSupervisorApplication.HasValue &&
                eligibleApplicationIds.Contains(s.IdSupervisorApplication.Value))
            .ToListAsync();
    }

    public async Task<List<SupervisorApplicationId>> GetAllApplicationIdsBySupervisorAsync(
        EmployeeId supervisorId)
    {
        return await _context.SupervisorApplications
            .AsNoTracking()
            .Where(a => a.IdEmployee == supervisorId)
            .Select(a => a.IdSupervisorApplication)
            .ToListAsync();
    }

    public async Task UpdateAsync(InterviewSlot slot)
    {
        _context.InterviewSlots.Update(slot);
        await _context.SaveChangesAsync();
    }

    public async Task AddInterviewAsync(Interview interview)
    {
        _context.Interviews.Add(interview);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(List<InterviewSlot> slots)
    {
        _context.InterviewSlots.AddRange(slots);
        await _context.SaveChangesAsync();
    }

    public async Task AddIntervalAsync(TimeInterval interval)
    {
        _context.TimeIntervals.Add(interval);
        await _context.SaveChangesAsync();
    }

    public async Task<List<InterviewSlot>> GetActiveByApplicationWithInterviewsAsync(
        SupervisorApplicationId supervisorApplicationId)
    {
        // БЕЗ AsNoTracking — сущности будут изменяться
        return await _context.InterviewSlots
            .Where(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.Status != InterviewSlotStatus.Cancelled)
            .Include(s => s.Interview)
            .ToListAsync();
    }

    public void RemoveInterview(Interview interview)
    {
        _context.Interviews.Remove(interview);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}