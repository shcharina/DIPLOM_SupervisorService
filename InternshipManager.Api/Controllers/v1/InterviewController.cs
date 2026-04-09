using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.DTOs.Interview;
using InternshipManager.Api.Services;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class InterviewController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SupervisorApplicationStatusService _statusService;
    public InterviewController(
        AppDbContext context,
        SupervisorApplicationStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    // GET api/v1/Interview/supervisor/{supervisorId}

    // Руководитель видит все свои назначенные собеседования

    [HttpGet("supervisor/{supervisorId}")] // здесь убран Guid !!!! :guid

    public async Task<IActionResult> GetBySupervisor(EmployeeId supervisorId)
    {
        var interviews = await _context.Interviews
            .Join(_context.InterviewSlots,
                i => i.IdInterviewSlot,
                s => s.IdInterviewSlot,
                (i, s) => new { Interview = i, Slot = s })
            .Where(x => x.Slot.IdEmployee == supervisorId)
            .Select(x => new
            {
                idInterviewSlot = x.Slot.IdInterviewSlot,
                idStudentApplication = x.Interview.IdStudentApplication,
                startTime = x.Slot.StartTime,
                endTime = x.Slot.EndTime,
                meetingPlace = x.Slot.MeetingPlace,
                interviewType = x.Interview.InterviewType,
                result = x.Interview.Result,
                comment = x.Interview.Comment,
                createdAt = x.Interview.CreatedAt,
                updatedAt = x.Interview.UpdatedAt
            })
            .ToListAsync();

        return Ok(interviews);
    }

    // GET api/v1/Interview/{id}

    // Одно собеседование по ID слота

    [HttpGet("{id:int}")] //:guid

    public async Task<IActionResult> GetById(InterviewSlotId id)
    {
        var interview = await _context.Interviews
            .Join(_context.InterviewSlots,
                i => i.IdInterviewSlot,
                s => s.IdInterviewSlot,
                (i, s) => new { Interview = i, Slot = s })
            .Where(x => x.Interview.IdInterviewSlot == id)
            .Select(x => new
            {
                idInterviewSlot = x.Slot.IdInterviewSlot,
                idStudentApplication = x.Interview.IdStudentApplication,
                startTime = x.Slot.StartTime,
                endTime = x.Slot.EndTime,
                meetingPlace = x.Slot.MeetingPlace,
                interviewType = x.Interview.InterviewType,
                result = x.Interview.Result,
                comment = x.Interview.Comment

            })
            .FirstOrDefaultAsync();

        if (interview == null)
            return NotFound(new { detail = "Собеседование не найдено" });

        return Ok(interview);

    }

    // PUT api/v1/Interview/{id}/result

    // Руководитель записывает результат собеседования

    [HttpPut("{id:int}/result")] //:guid

    public async Task<IActionResult> RecordResult(
        InterviewSlotId id,
        [FromBody] RecordInterviewResultDto dto)
    {
        var interview = await _context.Interviews.FindAsync(id);

        if (interview == null)
            return NotFound(new { detail = "Собеседование не найдено" });

        // Записываем результат
        interview.Result = dto.Result;
        interview.Comment = dto.Comment;
        interview.UpdatedAt = DateTime.UtcNow;

        // Находим связку студент-заявка
        var slot = await _context.InterviewSlots.FindAsync(id);

        if (slot?.IdSupervisorApplication != null)
        {
            var link = await _context.StudentSupervisorApplications
                .FirstOrDefaultAsync(s =>
                    s.IdSupervisorApplication == slot.IdSupervisorApplication &&
                    s.IdStudentApplication == interview.IdStudentApplication);

            if (link != null)
            {
                // проверка, что студент не отозвал заявку
                if (link.Status == StudentSupervisorApplicationStatus.Отказано)
                    return BadRequest(new
                    {
                        type = "business_error",
                        detail = "Студент отозвал заявку, результат собеседования записать невозможно"
                    });

                link.Status = dto.Result
                    ? StudentSupervisorApplicationStatus.ОформлениеДокументов
                    : StudentSupervisorApplicationStatus.Отказано;
                link.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Автоматическая проверка статуса заявки
            if (dto.Result && slot.IdSupervisorApplication.HasValue)
                await _statusService.CheckAndUpdateApplicationStatus(
                    slot.IdSupervisorApplication.Value);
        }

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idInterviewSlot = id,
            result = interview.Result,
            newStudentStatus = dto.Result
                ? StudentSupervisorApplicationStatus.ОформлениеДокументов.ToString()
                : StudentSupervisorApplicationStatus.Отказано.ToString(),
            message = dto.Result
                ? "Собеседование пройдено, студент переходит к оформлению документов"
                : "Собеседование не пройдено, студенту отказано"

        });

    }

}
