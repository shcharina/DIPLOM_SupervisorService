using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Services;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.DTOs.StudentSupervisorApplication;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class StudentSupervisorApplicationController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly SupervisorApplicationStatusService _statusService;
    private readonly ManagerApiClient _managerApi;
    private readonly StudentApiClient _studentApi;
    public StudentSupervisorApplicationController(
        AppDbContext context,
        SupervisorApplicationStatusService statusService,
        ManagerApiClient managerApi,
        StudentApiClient studentApi)
    {
        _context = context;
        _statusService = statusService;
        _managerApi = managerApi;
        _studentApi = studentApi;
    }

    // GET api/v1/StudentSupervisorApplication/{supervisorApplicationId}
    // Руководитель смотрит всех студентов по своей заявке

    [HttpGet("{supervisorApplicationId:int}")] //:guid

    public async Task<IActionResult> GetStudents(
        SupervisorApplicationId supervisorApplicationId,
        [FromQuery] StudentSupervisorApplicationStatus? status = null
        )
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
                status = s.Status
            })
            .ToListAsync();

        return Ok(students);
    }

        // POST api/v1/StudentSupervisorApplication

    // Менеджер назначает студента к заявке руководителя

    [HttpPost]

    public async Task<IActionResult> AssignStudent(
        [FromBody] AssignStudentDto dto)
    {
        // Проверяем что заявка руководителя существует и активна
        var supervisorApp = await _context.SupervisorApplications
            .FirstOrDefaultAsync(a =>
                a.IdSupervisorApplication == dto.IdSupervisorApplication &&
                a.Status == SupervisorApplicationStatus.Отправлена);

        if (supervisorApp == null)
            return NotFound(new
            {
                detail = "Активная заявка руководителя не найдена"
            });

        // Проверяем что такой связки ещё нет

        var exists = await _context.StudentSupervisorApplications
            .AnyAsync(s =>
                s.IdSupervisorApplication == dto.IdSupervisorApplication &&
                s.IdStudentApplication == dto.IdStudentApplication);

        if (exists)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Студент уже прикреплён к этой заявке"
            });

        var link = new StudentSupervisorApplication
        {
            IdSupervisorApplication = dto.IdSupervisorApplication,
            IdStudentApplication = dto.IdStudentApplication,
            Status = StudentSupervisorApplicationStatus.НаРассмотренииРуководителем

        };

        _context.StudentSupervisorApplications.Add(link);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idSupervisorApplication = link.IdSupervisorApplication,
            idStudentApplication = link.IdStudentApplication,
            status = (int)link.Status
        });
    }

    // GET api/v1/StudentSupervisorApplication/student/{studentApplicationId}
    // Студент видит свои статусы по всем заявкам руководителей

    [HttpGet("student/{studentApplicationId:int}")]

    public async Task<IActionResult> GetByStudent(
        StudentApplicationId studentApplicationId)
    {
        var links = await _context.StudentSupervisorApplications
            .Where(s => s.IdStudentApplication == studentApplicationId)
            .Select(s => new
            {
                idSupervisorApplication = s.IdSupervisorApplication,
                idStudentApplication = s.IdStudentApplication,
                status = s.Status,
                statusName = s.Status.ToString()
            })
            .ToListAsync();

        return Ok(links);
    }

    // GET api/v1/StudentSupervisorApplication/{supervisorApplicationId}/details/{studentApplicationId}
    // Руководитель видит полную информацию о студенте

    [HttpGet("{supervisorApplicationId:int}/details/{studentApplicationId:int}")]

    public async Task<IActionResult> GetStudentDetails(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        // Проверяем что студент прикреплён к этой заявке
        var link = await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);

        if (link == null)
            return NotFound(new { detail = "Студент не найден в этой заявке" });

        // Параллельно запрашиваем данные из других сервисов
        var studentApplicationTask = _studentApi
            .GetStudentApplicationAsync(studentApplicationId);
        var questionnaireTask = _studentApi
            .GetQuestionnaireAsync(studentApplicationId);
        var testingResultTask = _managerApi
            .GetTestingResultAsync(studentApplicationId);
        var managerInterviewTask = _managerApi
            .GetManagerInterviewResultAsync(studentApplicationId);

        // Ждём всех одновременно
        await Task.WhenAll(
            studentApplicationTask,
            questionnaireTask,
            testingResultTask,
            managerInterviewTask);

        return Ok(new
        {
            studentApplication = studentApplicationTask.Result,
            questionnaire = questionnaireTask.Result,      // null если нет
            testingResult = testingResultTask.Result,      // null если не проходил
            managerInterviewResult = managerInterviewTask.Result, // null если не было
            currentStatus = new
            {
                status = (int)link.Status,
                statusName = link.Status.ToString()
            }
        });
    }

    // PUT api/v1/StudentSupervisorApplication/{supervisorApplicationId}/{studentApplicationId}/accept
    // Руководитель берёт студента сразу на практику без собеседования

    [HttpPut("{supervisorApplicationId:int}/{studentApplicationId:int}/accept")] // :guid

    public async Task<IActionResult> AcceptWithoutInterview(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        var link = await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);

        if (link == null)
            return NotFound(new { detail = "Связка студент-заявка не найдена" });

        // проверка что студент не отозвал заявку
        if (link.Status == StudentSupervisorApplicationStatus.Отказано)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Студент отозвал заявку, дальнейшая работа с ним невозможна"
            });

        if (link.Status != StudentSupervisorApplicationStatus.НаРассмотренииРуководителем)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нельзя принять студента в статусе {link.Status}"
            });

        var currentAccepted = await _context.StudentSupervisorApplications
            .CountAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                (s.Status == StudentSupervisorApplicationStatus.ОформлениеДокументов ||
                 s.Status == StudentSupervisorApplicationStatus.Принят));

        var application = await _context.SupervisorApplications
            .FirstOrDefaultAsync(a => 
                a.IdSupervisorApplication == supervisorApplicationId);

        if (application != null && 
            currentAccepted >= application.RequestedStudentsCount)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Заявка уже набрала необходимое количество студентов"
            });

        link.Status = StudentSupervisorApplicationStatus.ОформлениеДокументов;
        
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

    [HttpPut("{supervisorApplicationId:int}/{studentApplicationId:int}/invite")] //:guid

    public async Task<IActionResult> InviteToInterview(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        var link = await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);

        if (link == null)
            return NotFound(new { detail = "Связка студент-заявка не найдена" });

        // Проверка что студент не отозвал заявку
        if (link.Status == StudentSupervisorApplicationStatus.Отказано)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Студент отозвал заявку, дальнейшая работа с ним невозможна"
            });

        if (link.Status != StudentSupervisorApplicationStatus.НаРассмотренииРуководителем)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нельзя пригласить студента в статусе {link.Status}"
            });

        link.Status = StudentSupervisorApplicationStatus.Собеседование;

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

    [HttpPut("{supervisorApplicationId:int}/{studentApplicationId:int}/reject")] //:guid

    public async Task<IActionResult> Reject(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        var link = await _context.StudentSupervisorApplications
            .FirstOrDefaultAsync(s =>
                s.IdSupervisorApplication == supervisorApplicationId &&
                s.IdStudentApplication == studentApplicationId);

        if (link == null)
            return NotFound(new { detail = "Связка студент-заявка не найдена" });

        link.Status = StudentSupervisorApplicationStatus.Отказано;

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