using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InternshipManager.Api.Data;
using InternshipManager.Api.Services;
using InternshipManager.Api.DTOs.SupervisorApplication;
using InternshipManager.Api.DTOs.Shared;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupervisorApplicationController : ControllerBase
{
    private readonly ISupervisorApplicationService _applicationService;
    private readonly SharedDbContext _sharedContext;
    private readonly ILogger<SupervisorApplicationController> _logger;
    
    public SupervisorApplicationController(
        ISupervisorApplicationService applicationService,
        SharedDbContext sharedContext,
        ILogger<SupervisorApplicationController> logger)
    {
        _applicationService = applicationService;
        _sharedContext = sharedContext;
        _logger = logger;
    }
    
    // ========== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ==========
    
    private async Task<SupervisorApplicationResponseDto> MapToResponseDto(SupervisorApplication application)
    {
        // Получаем данные из общей БД
        var employee = await _sharedContext.Employees
            .FirstOrDefaultAsync(e => e.IdEmployee == application.IdEmployee);
        
        EmployeeInfoDto? createdByInfo = null;
        if (application.IdCreatedBy.HasValue)
        {
            var createdBy = await _sharedContext.Employees
                .FirstOrDefaultAsync(e => e.IdEmployee == application.IdCreatedBy.Value);
            if (createdBy != null)
            {
                createdByInfo = new EmployeeInfoDto
                {
                    IdEmployee = createdBy.IdEmployee,
                    FirstName = createdBy.FirstName,
                    LastName = createdBy.LastName,
                    Patronymic = createdBy.Patronymic,
                    Position = createdBy.Position
                };
            }
        }
        
        var specialization = await _sharedContext.Specializations
            .FirstOrDefaultAsync(s => s.IdSpecialization == application.IdSpecialization);
        
        var department = await _sharedContext.Departments
            .FirstOrDefaultAsync(d => d.IdDepartment == application.IdDepartment);
        
        var address = await _sharedContext.Addresses
            .FirstOrDefaultAsync(a => a.IdAddress == application.IdAddress);
        
        ScheduledPracticeInfoDto? scheduledPracticeInfo = null;
        if (application.IdScheduledPractice.HasValue)
        {
            var scheduledPractice = await _sharedContext.ScheduledPractices
                .Include(sp => sp.Specialization)
                .Include(sp => sp.PracticeType)
                .FirstOrDefaultAsync(sp => sp.IdScheduledPractice == application.IdScheduledPractice.Value);
            
            if (scheduledPractice != null)
            {
                scheduledPracticeInfo = new ScheduledPracticeInfoDto
                {
                    IdScheduledPractice = scheduledPractice.IdScheduledPractice,
                    IdSpecialization = scheduledPractice.IdSpecialization,
                    SpecializationName = scheduledPractice.Specialization?.Name ?? string.Empty,
                    IdPracticeType = scheduledPractice.IdPracticeType,
                    PracticeTypeName = scheduledPractice.PracticeType?.Name ?? string.Empty,
                    StartDate = scheduledPractice.StartDate,
                    EndDate = scheduledPractice.EndDate
                };
            }
        }
        
        var currentStudentsCount = application.StudentSupervisorApplications
            .Count(ssa => ssa.Status == StudentApplicationStatus.Принят || 
                          ssa.Status == StudentApplicationStatus.ОформлениеДокументов);
        
        return new SupervisorApplicationResponseDto
        {
            IdSupervisorApplication = application.IdSupervisorApplication,
            IdEmployee = application.IdEmployee,
            EmployeeInfo = employee != null ? new EmployeeInfoDto
            {
                IdEmployee = employee.IdEmployee,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Patronymic = employee.Patronymic,
                Position = employee.Position
            } : null,
            IdCreatedBy = application.IdCreatedBy,
            CreatedByInfo = createdByInfo,
            IdSpecialization = application.IdSpecialization,
            SpecializationInfo = specialization != null ? new SpecializationInfoDto
            {
                IdSpecialization = specialization.IdSpecialization,
                Name = specialization.Name,
                Description = specialization.Description
            } : null,
            IdDepartment = application.IdDepartment,
            DepartmentInfo = department != null ? new DepartmentInfoDto
            {
                IdDepartment = department.IdDepartment,
                Name = department.Name
            } : null,
            IdAddress = application.IdAddress,
            AddressInfo = address != null ? new AddressInfoDto
            {
                IdAddress = address.IdAddress,
                IdDepartment = address.IdDepartment,
                FullAddress = address.FullAddress,
                City = address.City,
                PostalCode = address.PostalCode
            } : null,
            IdScheduledPractice = application.IdScheduledPractice,
            ScheduledPracticeInfo = scheduledPracticeInfo,
            StartDate = application.StartDate,
            EndDate = application.EndDate,
            RequestedStudentsCount = application.RequestedStudentsCount,
            CurrentStudentsCount = currentStudentsCount,
            PracticeFormat = application.PracticeFormat,
            IsPaid = application.IsPaid,
            Status = application.Status,
            CreatedAt = application.CreatedAt,
            UpdatedAt = application.UpdatedAt
        };
    }
    
    // ========== GET ЗАПРОСЫ ==========
    
    /// <summary>
    /// Получить все заявки руководителя
    /// </summary>
    [HttpGet("supervisor/{supervisorId}")]
    public async Task<ActionResult<IEnumerable<SupervisorApplicationResponseDto>>> GetBySupervisor(Guid supervisorId)
    {
        var applications = await _applicationService.GetApplicationsBySupervisorAsync(supervisorId);
        var result = new List<SupervisorApplicationResponseDto>();
        
        foreach (var app in applications)
        {
            result.Add(await MapToResponseDto(app));
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Получить заявку по ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SupervisorApplicationResponseDto>> GetById(Guid id)
    {
        var application = await _applicationService.GetApplicationByIdAsync(id);
        
        if (application == null)
            return NotFound($"Заявка с ID {id} не найдена");
        
        var result = await MapToResponseDto(application);
        return Ok(result);
    }
    
    /// <summary>
    /// Получить все активные заявки (для набора студентов)
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<SupervisorApplicationResponseDto>>> GetActive()
    {
        var applications = await _applicationService.GetActiveApplicationsAsync();
        var result = new List<SupervisorApplicationResponseDto>();
        
        foreach (var app in applications)
        {
            result.Add(await MapToResponseDto(app));
        }
        
        return Ok(result);
    }
    
    // ========== POST/PUT/DELETE ==========
    
    /// <summary>
    /// Создать новую заявку (статус "ШАБЛОН")
    /// </summary>
    [HttpPost("create/{supervisorId}")]
    public async Task<ActionResult<SupervisorApplicationResponseDto>> Create(
        Guid supervisorId, 
        [FromBody] CreateSupervisorApplicationDto dto)
    {
        try
        {
            // Проверяем, существует ли руководитель в общей БД
            var employee = await _sharedContext.Employees
                .FirstOrDefaultAsync(e => e.IdEmployee == supervisorId);
            
            if (employee == null)
                return BadRequest($"Руководитель с ID {supervisorId} не найден");
            
            var application = await _applicationService.CreateApplicationAsync(supervisorId, dto);
            var result = await MapToResponseDto(application);
            
            return CreatedAtAction(nameof(GetById), new { id = application.IdSupervisorApplication }, result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Обновить заявку (только для черновиков)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SupervisorApplicationResponseDto>> Update(
        Guid id, 
        [FromBody] UpdateSupervisorApplicationDto dto)
    {
        try
        {
            var application = await _applicationService.UpdateApplicationAsync(id, dto);
            
            if (application == null)
                return NotFound($"Заявка с ID {id} не найдена");
            
            var result = await MapToResponseDto(application);
            return Ok(result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Удалить заявку (только для черновиков)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        try
        {
            var deleted = await _applicationService.DeleteApplicationAsync(id);
            
            if (!deleted)
                return NotFound($"Заявка с ID {id} не найдена");
            
            return NoContent();
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    // ========== ИЗМЕНЕНИЕ СТАТУСА ==========
    
    /// <summary>
    /// Отправить заявку (ШАБЛОН → ОТПРАВЛЕНА)
    /// </summary>
    [HttpPut("{id}/send")]
    public async Task<IActionResult> Send(Guid id)
    {
        try
        {
            var result = await _applicationService.SendApplicationAsync(id);
            
            if (!result)
                return NotFound($"Заявка с ID {id} не найдена");
            
            return Ok(new { message = "Заявка успешно отправлена", status = "ОТПРАВЛЕНА" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Отменить заявку (→ ОТМЕНЕНА, студенты открепляются)
    /// </summary>
    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            var result = await _applicationService.CancelApplicationAsync(id);
            
            if (!result)
                return NotFound($"Заявка с ID {id} не найдена");
            
            return Ok(new { message = "Заявка отменена, студенты откреплены", status = "ОТМЕНЕНА" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
    
    /// <summary>
    /// Закрыть заявку досрочно (→ ЗАКРЫТА)
    /// </summary>
    [HttpPut("{id}/close")]
    public async Task<IActionResult> Close(Guid id)
    {
        try
        {
            var result = await _applicationService.CloseApplicationAsync(id);
            
            if (!result)
                return NotFound($"Заявка с ID {id} не найдена");
            
            return Ok(new { message = "Заявка закрыта досрочно", status = "ЗАКРЫТА" });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}