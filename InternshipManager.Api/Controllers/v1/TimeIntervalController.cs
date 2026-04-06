using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.DTOs.TimeInterval;

namespace InternshipManager.Api.Controllers;

[ApiController]

[Asp.Versioning.ApiVersion("1.0")]

[Route("api/v{version:apiVersion}/[controller]")]

public class TimeIntervalController : ControllerBase
{
    private readonly AppDbContext _context;
    public TimeIntervalController(AppDbContext context)
    {
        _context = context;
    }

    // POST api/v1/TimeInterval

    // Руководитель создаёт интервал → слоты сразу Подтверждены

    // Менеджер создаёт интервал → слоты ПредложенРуководителю

    [HttpPost]

    public async Task<IActionResult> Create([FromBody] CreateTimeIntervalDto dto)
    {
        var interval = new TimeInterval
        {
            IdEmployee = dto.IdEmployee,
            IdCreator = dto.IdCreator,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            MaxCount = dto.MaxCount,
            BreakDuration = dto.BreakDuration,
            CreatedAt = DateTime.UtcNow
        };

        _context.TimeIntervals.Add(interval);
        await _context.SaveChangesAsync();

        // Определяем статус слотов

        // Если создал сам руководитель (IdCreator == null или == IdEmployee)

        bool createdBySupervisor = dto.IdCreator == null 
                                || dto.IdCreator == dto.IdEmployee;

        var slotStatus = createdBySupervisor
            ? InterviewSlotStatus.Подтвержден      // руководитель → сразу подтверждён
            : InterviewSlotStatus.ПредложенРуководителю; // менеджер → на согласование

        // Нарезаем слоты

        var slots = GenerateSlots(interval, dto.MaxCount, dto.BreakDuration, slotStatus);
        _context.InterviewSlots.AddRange(slots);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            idInterval = interval.IdInterval,
            slotsCreated = slots.Count,
            slotStatus = slotStatus.ToString(),
            message = createdBySupervisor
                ? "Интервал создан, слоты подтверждены"
                : "Интервал создан, слоты отправлены на согласование руководителю"
        });
    }

    // Вспомогательный метод нарезки слотов

    private List<InterviewSlot> GenerateSlots(
        TimeInterval interval,
        int count,
        TimeSpan? breakDuration,
        InterviewSlotStatus status)
    {
        var slots = new List<InterviewSlot>();
        var totalDuration = interval.EndTime - interval.StartTime;
        var breakTime = breakDuration ?? TimeSpan.Zero;
        var slotDuration = (totalDuration - breakTime * (count - 1)) / count;
        var currentStart = interval.StartTime;
        for (int i = 0; i < count; i++)
        {
            var slotEnd = currentStart + slotDuration;
    
            slots.Add(new InterviewSlot
            {
                IdEmployee = interval.IdEmployee,
                IdCreator = interval.IdCreator,
                IdInterval = interval.IdInterval,
                StartTime = currentStart,
                EndTime = slotEnd,
                Status = status,
                CreatedAt = DateTime.UtcNow
            });

            currentStart = slotEnd + breakTime;
        }

        return slots;

    }

}

