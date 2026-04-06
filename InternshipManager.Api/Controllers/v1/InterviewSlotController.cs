using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.DTOs.InterviewSlot;

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

    [HttpGet("pending/{supervisorId:guid}")]

    public async Task<IActionResult> GetPending(Guid supervisorId)
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

    public async Task<IActionResult> Confirm(int id)
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
        slot.UpdatedAt = DateTime.UtcNow;

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

    public async Task<IActionResult> Reject(int id, [FromBody] RejectSlotDto dto)
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
        slot.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idInterviewSlot = slot.IdInterviewSlot,
            status = slot.Status,
            rejectionComment = slot.RejectionComment,
            message = "Слот отклонён"
        });

    }

    // PUT api/v1/InterviewSlot/{id}/publish

    // Публикация слота — становится доступным студентам

    [HttpPut("{id}/publish")]

    public async Task<IActionResult> Publish(
        int id,
        [FromQuery] Guid supervisorApplicationId)
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

        // Проверяем что заявка существует
        var application = await _context.SupervisorApplications
            .FirstOrDefaultAsync(a => a.IdSupervisorApplication == supervisorApplicationId);
            
        if (application == null)
            return NotFound(new { detail = "Заявка не найдена" });
            
        // Получаем студентов со статусом Собеседование по этой заявке
        // Теперь работает правильно после исправления типа в модели
        
        var eligibleStudents = await _context.StudentSupervisorApplications
            .Where(s => s.IdSupervisorApplication == supervisorApplicationId
            && s.Status == StudentSupervisorApplicationStatus.Собеседование) // ✅ типы совпадают
            .Select(s => s.IdStudentApplication)
            .ToListAsync();
            
        if (!eligibleStudents.Any())
            return BadRequest(new
            {
                type = "business_error",
                detail = "Нет студентов со статусом Собеседование по этой заявке"
            });
        
        
        
        // Привязываем слот к заявке
        
        slot.IdSupervisorApplication = supervisorApplicationId;

        slot.Status = InterviewSlotStatus.Опубликован;
        slot.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idInterviewSlot = slot.IdInterviewSlot,
            status = slot.Status,
            supervisorApplicationId,
            // Сервис практиканта читает этот список и показывает слот этим студентам
            studentsToNotify = eligibleStudents,
            studentsCount = eligibleStudents.Count,
            message = "Слот опубликован, доступен студентам"
        });

    }

    // GET api/v1/InterviewSlot/available/{supervisorId}

    // Студент видит опубликованные слоты руководителя

    [HttpGet("available/{supervisorId:guid}")]

    public async Task<IActionResult> GetAvailable(Guid supervisorId)
    {
        var slots = await _context.InterviewSlots
            .Where(s => s.IdEmployee == supervisorId
                     && s.Status == InterviewSlotStatus.Опубликован)
            .Select(s => new
            {
                id = s.IdInterviewSlot,
                startTime = s.StartTime,
                endTime = s.EndTime,
                meetingPlace = s.MeetingPlace
            })
            .ToListAsync();

        return Ok(slots);

    }


}

