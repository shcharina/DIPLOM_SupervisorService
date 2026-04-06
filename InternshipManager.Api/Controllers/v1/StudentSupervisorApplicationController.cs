using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Controllers;

[ApiController]

[Asp.Versioning.ApiVersion("1.0")]

[Route("api/v{version:apiVersion}/[controller]")]

public class StudentSupervisorApplicationController : ControllerBase
{
    private readonly AppDbContext _context;
    public StudentSupervisorApplicationController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/v1/StudentSupervisorApplication/{supervisorApplicationId}

    // Руководитель смотрит всех студентов по своей заявке

    [HttpGet("{supervisorApplicationId:guid}")]

    public async Task<IActionResult> GetStudents(Guid supervisorApplicationId)
    {
        var students = await _context.StudentSupervisorApplications
            .Where(s => s.IdSupervisorApplication == supervisorApplicationId)
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

