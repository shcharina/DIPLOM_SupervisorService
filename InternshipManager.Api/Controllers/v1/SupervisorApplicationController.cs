using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.DTOs.SupervisorApplication;
using InternshipManager.Api.Models.Shared;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class SupervisorApplicationController : ControllerBase
{

    private readonly AppDbContext _context;
    public SupervisorApplicationController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/v1/SupervisorApplication/supervisor/{supervisorId}

    [HttpGet("supervisor/{supervisorId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetBySupervisor(

        Guid supervisorId,

        [FromQuery] int page = 1,

        [FromQuery] int pageSize = 20,

        [FromQuery] SupervisorApplicationStatus? status = null)

    {

        if (page < 1 || pageSize < 1 || pageSize > 100)

            return BadRequest(new { detail = "Некорректные параметры пагинации" });

        var query = _context.SupervisorApplications

            .Where(a => a.IdEmployee == supervisorId);

        if (status.HasValue)

            query = query.Where(a => a.Status == status.Value);

        var totalItems = await query.CountAsync();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var data = await query

            .OrderByDescending(a => a.CreatedAt)

            .Skip((page - 1) * pageSize)

            .Take(pageSize)

            .ToListAsync();

        return Ok(new

        {

            data,

            pagination = new

            {

                currentPage = page,

                pageSize,

                totalPages,

                totalItems,

                hasNextPage = page < totalPages,

                hasPreviousPage = page > 1

            }

        });

    }

    // GET api/v1/SupervisorApplication/{id}

    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetById(Guid id)
    {
        var application = await _context.SupervisorApplications
            .FirstOrDefaultAsync(a => a.IdSupervisorApplication == id);

        if (application == null)
            return NotFound(new { detail = "Заявка не найдена" });

        return Ok(application);
    }

    // GET api/v1/SupervisorApplication/active

    [HttpGet("active")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetActive(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)

    {
        var query = _context.SupervisorApplications
            .Where(a => a.Status == SupervisorApplicationStatus.Отправлена);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        var data = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new
        {
            data,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalPages,
                totalItems,
                hasNextPage = page < totalPages,
                hasPreviousPage = page > 1
            }
        });

    }

    // POST api/v1/SupervisorApplication

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]

public async Task<IActionResult> Create([FromBody] CreateSupervisorApplicationDto dto)
{
    var supervisor = await _context.Employees
        .FirstOrDefaultAsync(e =>
            e.IdEmployee == dto.SupervisorId &&
            e.Role == EmployeeRole.Supervisor);

    if (supervisor == null)
        return NotFound(new
        {
            type = "not_found",
            detail = "Руководитель с таким ID не найден"
        }
        );
    
    int idSpecialization;
    DateTime? startDate;
    DateTime? endDate;

    if (dto.IdScheduledPractice.HasValue)
        {
            //Берем из БД данные об практиках из расписания
            var scheduledPractice = await _context.ScheduledPractices
                .FirstOrDefaultAsync( sp => sp.IdScheduledPractice == dto.IdScheduledPractice.Value);
            if (scheduledPractice == null)
                return NotFound(new
                {
                    type = "not_found",
                    detail = "Практика из расписания не найдена"
                });

            // Берём данные из расписания
            idSpecialization = scheduledPractice.IdSpecialization;
            startDate = scheduledPractice.StartDate;
            endDate = scheduledPractice.EndDate;   
        
        }
    else
        {
            // Ручное заполнение — все поля обязательны
            if (dto.IdSpecialization == null || dto.StartDate == null || dto.EndDate == null)
                    return BadRequest(new
                    {
                        type = "validation_error",
                        detail = "Если не указана практика из расписания, нужно указать специализацию, дату начала и дату конца"
                    });

                // Дата окончания должна быть позже даты начала

                if (dto.StartDate >= dto.EndDate)
                    return BadRequest(new
                    {
                        type = "validation_error",
                        detail = "Дата окончания должна быть позже даты начала"
                    });

                idSpecialization = dto.IdSpecialization.Value;
                startDate = dto.StartDate;
                endDate = dto.EndDate;
        }

    // Создаём модель из DTO

    var application = new Models.Supervisor.SupervisorApplication
    {
        IdSupervisorApplication = Guid.NewGuid(),
        IdEmployee = dto.SupervisorId,
        IdSpecialization = idSpecialization,
        IdDepartment = dto.IdDepartment,
        IdAddress = dto.IdAddress,
        IdScheduledPractice = dto.IdScheduledPractice,
        StartDate = startDate,
        EndDate = endDate,
        RequestedStudentsCount = dto.RequestedStudentsCount,
        PracticeFormat = dto.PracticeFormat,
        IsPaid = dto.IsPaid,
        Status = SupervisorApplicationStatus.Шаблон,
        CreatedAt = DateTime.UtcNow
    };

    _context.SupervisorApplications.Add(application);
    await _context.SaveChangesAsync();

    // Возвращаем DTO, а не модель

    return CreatedAtAction(nameof(GetById),
        new { id = application.IdSupervisorApplication, version = "1" },
        ToResponseDto(application));

}


    // PUT api/v1/SupervisorApplication/{id}

[HttpPut("{id:guid}")]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status400BadRequest)]
[ProducesResponseType(StatusCodes.Status404NotFound)]

public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSupervisorApplicationDto dto)
{
    var application = await _context.SupervisorApplications.FindAsync(id);

    if (application == null)
        return NotFound(new { detail = "Заявка не найдена" });

    if (application.Status != SupervisorApplicationStatus.Шаблон)
        return BadRequest(new
        {
            type = "business_error",
            detail = $"Нельзя редактировать заявку в статусе {application.Status}"
        });

    // Обновляем только те поля, которые пришли

    if (dto.IdSpecialization.HasValue) application.IdSpecialization = dto.IdSpecialization.Value;
    if (dto.IdDepartment.HasValue) application.IdDepartment = dto.IdDepartment.Value;
    if (dto.IdAddress.HasValue) application.IdAddress = dto.IdAddress.Value;
    if (dto.IdScheduledPractice.HasValue) application.IdScheduledPractice = dto.IdScheduledPractice;
    if (dto.RequestedStudentsCount.HasValue) application.RequestedStudentsCount = dto.RequestedStudentsCount.Value;
    if (dto.PracticeFormat.HasValue) application.PracticeFormat = dto.PracticeFormat.Value;
    if (dto.IsPaid.HasValue) application.IsPaid = dto.IsPaid.Value;
    if (dto.StartDate.HasValue) application.StartDate = dto.StartDate;
    if (dto.EndDate.HasValue) application.EndDate = dto.EndDate;

    application.UpdatedAt = DateTime.UtcNow;
    await _context.SaveChangesAsync();

    return Ok(ToResponseDto(application));
}

    // DELETE api/v1/SupervisorApplication/{id}

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> Delete(Guid id)
    {
        var application = await _context.SupervisorApplications.FindAsync(id);
        if (application == null)
            return NotFound(new { detail = "Заявка не найдена" });

        if (application.Status != SupervisorApplicationStatus.Шаблон)
            return BadRequest(new
            {
                type = "business_error",
                detail = "Удалить можно только заявку в статусе Шаблон"
            });

        _context.SupervisorApplications.Remove(application);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PUT api/v1/SupervisorApplication/{id}/send

    [HttpPut("{id:guid}/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> Send(Guid id)
    {
        var application = await _context.SupervisorApplications.FindAsync(id);

        if (application == null)
            return NotFound(new { detail = "Заявка не найдена" });

        if (application.Status != SupervisorApplicationStatus.Шаблон)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Невозможно отправить заявку в статусе {application.Status}"
            });

        application.Status = SupervisorApplicationStatus.Отправлена;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idSupervisorApplication = application.IdSupervisorApplication,
            status = application.Status,
            message = "Заявка успешно отправлена"
        });

    }

    // PUT api/v1/SupervisorApplication/{id}/cancel

    [HttpPut("{id:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> Cancel(Guid id)
    {
        var application = await _context.SupervisorApplications.FindAsync(id);
        
        if (application == null)
            return NotFound(new { detail = "Заявка не найдена" });

        if (application.Status != SupervisorApplicationStatus.Шаблон &&
            application.Status != SupervisorApplicationStatus.Отправлена)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нельзя отменить заявку в статусе {application.Status}"
            });

        var previousStatus = application.Status;
        application.Status = SupervisorApplicationStatus.Отменена;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idSupervisorApplication = application.IdSupervisorApplication,
            status = application.Status,
            previousStatus,
            message = "Заявка отменена"
        });

    }

    // PUT api/v1/SupervisorApplication/{id}/close

    [HttpPut("{id:guid}/close")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> Close(Guid id)
    {
        var application = await _context.SupervisorApplications.FindAsync(id);
        
        if (application == null)
            return NotFound(new { detail = "Заявка не найдена" });

        if (application.Status != SupervisorApplicationStatus.Отправлена)
            return BadRequest(new
            {
                type = "business_error",
                detail = $"Нельзя закрыть заявку в статусе {application.Status}"
            });

        application.Status = SupervisorApplicationStatus.Закрыта;
        application.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            idSupervisorApplication = application.IdSupervisorApplication,
            status = application.Status,
            message = "Заявка закрыта досрочно"
        });

    }

    //Вспомогательный метод ToResponseDto
    private static SupervisorApplicationResponseDto ToResponseDto(
        Models.Supervisor.SupervisorApplication app)
    {
        return new SupervisorApplicationResponseDto
        {
            IdSupervisorApplication = app.IdSupervisorApplication,
            IdEmployee = app.IdEmployee,
            IdCreatedBy = app.IdCreatedBy,
            IdSpecialization = app.IdSpecialization,
            IdDepartment = app.IdDepartment,
            IdAddress = app.IdAddress,
            IdScheduledPractice = app.IdScheduledPractice,
            StartDate = app.StartDate,
            EndDate = app.EndDate,
            RequestedStudentsCount = app.RequestedStudentsCount,
            PracticeFormat = app.PracticeFormat,
            IsPaid = app.IsPaid,
            Status = app.Status,
            CreatedAt = app.CreatedAt,
            UpdatedAt = app.UpdatedAt,
            IsCreatedByManager = app.IdCreatedBy != null && app.IdCreatedBy != app.IdEmployee,
            IsFromSchedule = app.IdScheduledPractice != null
        };
    }

}