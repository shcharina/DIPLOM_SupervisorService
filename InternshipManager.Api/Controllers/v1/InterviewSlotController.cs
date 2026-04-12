using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.DTOs.InterviewSlot;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class InterviewSlotController : ControllerBase
{
    private readonly AppDbContext _context;

    public InterviewSlotController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/v1/InterviewSlot/pending/{supervisorId}

    // Руководитель видит слоты на согласование (предложенные менеджером)

    [HttpGet("pending/{supervisorId:int}")] //:guid

    public async Task<IActionResult> GetPending(EmployeeId supervisorId)
    {
        var slots = await _context.InterviewSlots
            .Where(s => s.IdEmployee == supervisorId
                     && s.Status == InterviewSlotStatus.ПредложенРуководителю)
            .ToListAsync();

        return Ok(slots);
    }

    // PUT api/v1/InterviewSlot/{id}/confirm

    // Руководитель подтверждает слот

    [HttpPut("{id}/confirm")]

    public async Task<IActionResult> Confirm(InterviewSlotId id)
    {
        var slot = await _context.InterviewSlots.FindAsync(id);
        if (slot == null)
            return NotFound(new { detail = "Слот не найден" });

        if (slot.Status != InterviewSlotStatus.ПредложенРуководителю)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нельзя подтвердить слот в статусе {slot.Status}"

            });

        slot.Status = InterviewSlotStatus.Подтвержден;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idInterviewSlot = slot.IdInterviewSlot,
            status = slot.Status,
            message = "Слот подтверждён"

        });

    }

    // PUT api/v1/InterviewSlot/{id}/reject

    // Руководитель отклоняет слот с комментарием

    [HttpPut("{id}/reject")]

    public async Task<IActionResult> Reject(InterviewSlotId id, [FromBody] RejectSlotDto dto)
    {
        var slot = await _context.InterviewSlots.FindAsync(id);
        if (slot == null)
            return NotFound(new { detail = "Слот не найден" });

        if (slot.Status != InterviewSlotStatus.ПредложенРуководителю)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нельзя отклонить слот в статусе {slot.Status}"
            });

        slot.Status = InterviewSlotStatus.Отменен;
        slot.RejectionComment = dto.Comment;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idInterviewSlot = slot.IdInterviewSlot,
            status = slot.Status,
            rejectionComment = slot.RejectionComment,
            message = "Слот отклонён"
        });

    }

    // PUT api/v1/InterviewSlot/{id}/publish?supervisorApplicationId={guid} - одна заявка
    // PUT api/v1/InterviewSlot/{id}/publish - все заявки

    // Публикация слота - становится доступным студентам

    [HttpPut("{id}/publish")]

    public async Task<IActionResult> Publish(
        InterviewSlotId id,
        [FromQuery] SupervisorApplicationId? supervisorApplicationId = null)
    {
        var slot = await _context.InterviewSlots.FindAsync(id);

        if (slot == null)
            return NotFound(new { detail = "Слот не найден" });

        if (slot.Status != InterviewSlotStatus.Подтвержден)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Публиковать можно только подтверждённые слоты"
            });

        List<StudentApplicationId> eligibleStudents;
        string scope;

        if (supervisorApplicationId.HasValue)
        {
            // Проверяем что заявка существует
            var application = await _context.SupervisorApplications
                .FirstOrDefaultAsync(a =>
                   a.IdSupervisorApplication == supervisorApplicationId);

            if (application == null)
                return NotFound(new { detail = "Заявка не найдена" });

            eligibleStudents = await _context.StudentSupervisorApplications
                .Where(s => 
                    s.IdSupervisorApplication == supervisorApplicationId.Value &&
                    s.Status == StudentSupervisorApplicationStatus.Собеседование)
                    .Select(s => s.IdStudentApplication)
                    .ToListAsync();
            
            scope = $"по заявке {supervisorApplicationId.Value}";

        }
        else
        {
            var allApplicationIds = await _context.SupervisorApplications
                .Where(a => a.IdEmployee == slot.IdEmployee)
                .Select(a => a.IdSupervisorApplication)
                .ToListAsync();
            
            eligibleStudents = await _context.StudentSupervisorApplications
                .Where(s =>
                    allApplicationIds.Contains(s.IdSupervisorApplication) &&
                    s.Status == StudentSupervisorApplicationStatus.Собеседование)
                    .Select(s => s.IdStudentApplication)
                    .Distinct()
                    .ToListAsync();
                scope = "по всем заявкам руководителя";
        }

        if (!eligibleStudents.Any())
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нет студентов со статусом Собеседование {scope}"
            });
        
        // Привязываем слот к заявке
        
        slot.IdSupervisorApplication = supervisorApplicationId; // null если все заявки

        slot.Status = InterviewSlotStatus.Опубликован;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idInterviewSlot = slot.IdInterviewSlot,
            status = slot.Status,
            scope,
            // Сервис практиканта читает этот список и показывает слот этим студентам
            studentsToNotify = eligibleStudents,
            studentsCount = eligibleStudents.Count,
            message = $"Слот опубликован для {eligibleStudents.Count} студентов {scope}"
        });

    }

    // GET api/v1/InterviewSlot/available/{supervisorId}
    // Студент видит опубликованные слоты руководителя

    [HttpGet("available/{supervisorId:int}")] //:guid

    public async Task<IActionResult> GetAvailable(
        EmployeeId supervisorId,
        [FromQuery] StudentApplicationId studentApplicationId)
    {
        // Ищем заявки руководителя, где студент приглашен на собеседование
        var eligibleApplicationIds = await _context.StudentSupervisorApplications
        .Where(s =>
                s.IdStudentApplication == studentApplicationId &&
                s.Status == StudentSupervisorApplicationStatus.Собеседование)
            .Select(s => s.IdSupervisorApplication)
            .ToListAsync();

        var slots = await _context.InterviewSlots
            .Where(s =>
                    s.IdEmployee == supervisorId &&
                    s.Status == InterviewSlotStatus.Опубликован &&
                    s.IdSupervisorApplication.HasValue &&
                    eligibleApplicationIds.Contains(s.IdSupervisorApplication.Value))
            .Select(s => new
            {
                idInterviewSlot = s.IdInterviewSlot,
                startTime = s.StartTime,
                endTime = s.EndTime,
                meetingPlace = s.MeetingPlace
            })
            .ToListAsync();

        return Ok(slots);
    }

    // PUT api/v1/InterviewSlot/{id}/book
    // Студент бронирует слот
    [HttpPut("{id:int}/book")]

    public async Task<IActionResult> Book(
        InterviewSlotId id,
        [FromBody] BookSlotDto dto)
    {
        var slot = await _context.InterviewSlots
            .Include(s => s.Interview)
            .FirstOrDefaultAsync(s => s.IdInterviewSlot == id);

        if (slot == null)
            return NotFound(new { detail = "Слот не найден" });
        if (slot.Status != InterviewSlotStatus.Опубликован)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Слот недоступен для бронирования"
            });

        // Проверяем право студента на этот слот
        if (slot.IdSupervisorApplication.HasValue)
        {
            var isEligible = await _context.StudentSupervisorApplications
                .AnyAsync(s =>
                    s.IdSupervisorApplication == slot.IdSupervisorApplication &&
                    s.IdStudentApplication == dto.IdStudentApplication &&
                    s.Status == StudentSupervisorApplicationStatus.Собеседование);

            if (!isEligible)
                return BadRequest(new
                {
                    type = "business_error",
                    detail = "Студент не может записаться на этот слот"
                });
        }
        slot.Status = InterviewSlotStatus.Занят;

        var interview = new Interview
        {
            IdInterviewSlot = id,
            IdStudentApplication = dto.IdStudentApplication,
            InterviewType = InterviewType.Руководитель,
            Status = InterviewStatus.Назначено,
            Result = false
        };

        _context.Interviews.Add(interview);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idInterviewSlot = id,
            message = $"Собеседование назначено на " +
                      $"{slot.StartTime:dd MMMM} в " +
                      $"{slot.StartTime:HH:mm}"
        });
    }
}