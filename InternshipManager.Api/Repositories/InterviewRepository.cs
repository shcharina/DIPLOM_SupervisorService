using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;

namespace InternshipManager.Api.Repositories;

public class InterviewRepository : IInterviewRepository
{
    private readonly AppDbContext _context;

    public InterviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<object?> GetBySupervisorAsync(EmployeeId supervisorId)
    {
        return await _context.Interviews
            .Join(_context.InterviewSlots,
                i => i.IdInterviewSlot,
                s => s.IdInterviewSlot,
                (i, s) => new { Interview = i, Slot = s })
            .Where(x => x.Slot.IdEmployee == supervisorId)
            .Select(x => new
            {
                idInterviewSlot        = x.Slot.IdInterviewSlot,
                idStudentApplication   = x.Interview.IdStudentApplication,
                startTime              = x.Slot.StartTime,
                endTime                = x.Slot.EndTime,
                meetingPlace           = x.Slot.MeetingPlace,
                interviewType          = x.Interview.InterviewType,
                result                 = x.Interview.Result,
                comment                = x.Interview.Comment,
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<object?> GetByIdAsync(InterviewSlotId id)
    {
        return await _context.Interviews
            .Join(_context.InterviewSlots,
                i => i.IdInterviewSlot,
                s => s.IdInterviewSlot,
                (i, s) => new { Interview = i, Slot = s })
            .Where(x => x.Interview.IdInterviewSlot == id)
            .Select(x => new
            {
                idInterviewSlot        = x.Slot.IdInterviewSlot,
                idStudentApplication   = x.Interview.IdStudentApplication,
                startTime              = x.Slot.StartTime,
                endTime                = x.Slot.EndTime,
                meetingPlace           = x.Slot.MeetingPlace,
                interviewType          = x.Interview.InterviewType,
                result                 = x.Interview.Result,
                comment                = x.Interview.Comment,
            })
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }

    public async Task<Interview?> FindAsync(InterviewSlotId id)
    {
        return await _context.Interviews.FindAsync(id);
    }

    public async Task<InterviewSlot?> FindSlotAsync(InterviewSlotId id)
    {
        return await _context.InterviewSlots.FindAsync(id);
    }

    public async Task<StudentSupervisorApplication?> FindLinkAsync(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        return await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication    == studentApplicationId);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}