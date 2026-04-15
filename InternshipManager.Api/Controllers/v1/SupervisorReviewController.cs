using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.DTOs.SupervisorReview;
using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class SupervisorReviewController : ControllerBase
{
    private readonly AppDbContext _context;
    public SupervisorReviewController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/v1/SupervisorReview/pending/{supervisorId}
    
    // Руководитель видит список студентов которым нужен отзыв
    
    [HttpGet("pending/{supervisorId:int}")] // :guid

    public async Task<IActionResult> GetPendingReviews(EmployeeId supervisorId)
    {
        // Все заявки этого руководителя где практика закончилась
        var completedApplicationIds = await _context.SupervisorApplications
            .Where(a =>  
                a.IdEmployee == supervisorId &&
                a.EndDate != null &&
                a.EndDate <= DateTime.UtcNow &&
                a.Status == SupervisorApplicationStatus.Satisfied)

            .Select(a => a.IdSupervisorApplication)
            .ToListAsync();

        // Студенты которые завершили практику
        var completedStudents = await _context.StudentSupervisorApplications
            .Where(s =>
                completedApplicationIds.Contains(s.IdSupervisorApplication) &&
                s.Status == StudentSupervisorApplicationStatus.Принят)
            .ToListAsync();

        // Убираем тех на кого отзыв уже есть

        var existingReviews = await _context.SupervisorReviews
            .Where(r => r.IdEmployee == supervisorId)
            .Select(r => r.IdStudentApplication)
            .ToListAsync();

        var pendingReviews = completedStudents
            .Where(s => !existingReviews.Contains(s.IdStudentApplication))
            .Select(s => new
            {
                idStudentApplication = s.IdStudentApplication,
                idSupervisorApplication = s.IdSupervisorApplication,
                message = "Необходимо оставить отзыв"
            })
            .ToList();

        return Ok(new
        {
            pendingCount = pendingReviews.Count,
            students = pendingReviews
        });

    }

    // POST api/v1/SupervisorReview

    // Руководитель оставляет отзыв

    [HttpPost]

    public async Task<IActionResult> Create([FromBody] CreateSupervisorReviewDto dto)
    {
        // Проверяем что студент действительно завершил практику у этого руководителя
        var studentCompleted = await _context.StudentSupervisorApplications
            .AnyAsync(s =>
                s.IdStudentApplication == dto.IdStudentApplication &&
                s.Status == StudentSupervisorApplicationStatus.Принят);

        if (!studentCompleted)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Нельзя оставить отзыв — студент не завершил практику"
            });

        // Проверяем что отзыв ещё не оставлен

        var reviewExists = await _context.SupervisorReviews
            .AnyAsync(r =>
                r.IdEmployee == dto.IdEmployee &&
                r.IdStudentApplication == dto.IdStudentApplication);

        if (reviewExists)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Отзыв об этом студенте уже оставлен"
            });

        var review = new SupervisorReview
        {
            IdEmployee = dto.IdEmployee,
            IdStudentApplication = dto.IdStudentApplication,
            RecommendedForEmployment = dto.RecommendedForEmployment,
            PvScore = dto.PvScore,
            SkillsScore = dto.SkillsScore,
            IndependenceScore = dto.IndependenceScore,
            TeamworkScore = dto.TeamworkScore,
            OverallScore = dto.OverallScore,
            Comment = dto.Comment,

        };

        _context.SupervisorReviews.Add(review);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idEmployee = review.IdEmployee,
            idStudentApplication = review.IdStudentApplication,
            message = "Отзыв успешно оставлен"

        });
    }
    
    // GET api/v1/SupervisorReview/{idEmployee}/{idStudentApplication}
    // Менеджер или студент получает отзыв руководителя

    [HttpGet("{idEmployee:int}/{idStudentApplication:int}")]

    public async Task<IActionResult> GetReview(
        EmployeeId idEmployee,
        StudentApplicationId idStudentApplication)
    {
        var review = await _context.SupervisorReviews
            .FirstOrDefaultAsync(r =>
                r.IdEmployee == idEmployee &&
                r.IdStudentApplication == idStudentApplication);

        if (review == null)
            return NotFound(new { detail = "Отзыв не найден" });

        return Ok(new
        {
            idEmployee = review.IdEmployee,
            idStudentApplication = review.IdStudentApplication,
            recommendedForEmployment = review.RecommendedForEmployment,
            pvScore = review.PvScore,
            skillsScore = review.SkillsScore,
            independenceScore = review.IndependenceScore,
            teamworkScore = review.TeamworkScore,
            overallScore = review.OverallScore,
            comment = review.Comment
        });
    }

}