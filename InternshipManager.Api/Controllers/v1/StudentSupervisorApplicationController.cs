using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Services;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class StudentSupervisorApplicationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SupervisorApplicationStatusService _statusService;
    public StudentSupervisorApplicationController(
        AppDbContext context,
        SupervisorApplicationStatusService statusService)
    {
        _context = context;
        _statusService = statusService;
    }

    // GET api/v1/StudentSupervisorApplication/{supervisorApplicationId}

    // Руководитель смотрит всех студентов по своей заявке

    [HttpGet("{supervisorApplicationId:guid}")]

    public async Task<IActionResult> GetStudents(
        Guid supervisorApplicationId,
        [FromQuery] StudentSupervisorApplicationStatus? status = null        )
    {
        var query = _context.StudentSupervisorApplications
            .Where(s => s.IdSupervisorApplication == supervisorApplicationId);
        
        // Фильтр по статусу (необязательный)
        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);
        
        var students = await query
            .Select(s => new
            {
                idStudentApplication = s.IdStudentApplication,
                status = s.Status,
                assignedAt = s.AssignedAt,
                updatedAt = s.UpdatedAt
            })
            .ToListAsync();

        return Ok(students);
    }

    // PUT api/v1/StudentSupervisorApplication/{supervisorApplicationId}/{studentApplicationId}/accept

    // Руководитель берёт студента сразу на практику без собеседования

    [HttpPut("{supervisorApplicationId:guid}/{studentApplicationId:guid}/accept")]

    public async Task<IActionResult> AcceptWithoutInterview(
        Guid supervisorApplicationId,
        Guid studentApplicationId)
    {
        var link = await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);

        if (link == null)
            return NotFound(new { detail = "Связка студент-заявка не найдена" });

        if (link.Status != StudentSupervisorApplicationStatus.НаРассмотренииРуководителем)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нельзя принять студента в статусе {link.Status}"
            });

        link.Status = StudentSupervisorApplicationStatus.ОформлениеДокументов;
        link.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Автоматическая проверка статуса заявки
        await _statusService.CheckAndUpdateApplicationStatus(supervisorApplicationId);

        return Ok(new
        {
            idSupervisorApplication = supervisorApplicationId,
            idStudentApplication = studentApplicationId,
            status = link.Status,
            // Сервис студента читает это поле и открывает загрузку документов
            message = "Студент принят на практику, переходит к оформлению документов"
        });

    }
    
    // PUT api/v1/StudentSupervisorApplication/{supervisorApplicationId}/{studentApplicationId}/invite

    // Руководитель приглашает студента на собеседование

    [HttpPut("{supervisorApplicationId:guid}/{studentApplicationId:guid}/invite")]

    public async Task<IActionResult> InviteToInterview(
        Guid supervisorApplicationId,
        Guid studentApplicationId)
    {
        var link = await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);

        if (link == null)
            return NotFound(new { detail = "Связка студент-заявка не найдена" });

        if (link.Status != StudentSupervisorApplicationStatus.НаРассмотренииРуководителем)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нельзя пригласить студента в статусе {link.Status}"
            });

        link.Status = StudentSupervisorApplicationStatus.Собеседование;
        link.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idSupervisorApplication = supervisorApplicationId,
            idStudentApplication = studentApplicationId,
            status = link.Status,
            message = "Студент приглашён на собеседование"
        });

    }

    // PUT api/v1/StudentSupervisorApplication/{supervisorApplicationId}/{studentApplicationId}/reject

    // Руководитель отказывает студенту

    [HttpPut("{supervisorApplicationId:guid}/{studentApplicationId:guid}/reject")]

    public async Task<IActionResult> Reject(
        Guid supervisorApplicationId,
        Guid studentApplicationId)
    {
        var link = await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);

        if (link == null)
            return NotFound(new { detail = "Связка студент-заявка не найдена" });

        link.Status = StudentSupervisorApplicationStatus.Отказано;
        link.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idSupervisorApplication = supervisorApplicationId,
            idStudentApplication = studentApplicationId,
            status = link.Status,
            message = "Студенту отказано"
        });

    }

}