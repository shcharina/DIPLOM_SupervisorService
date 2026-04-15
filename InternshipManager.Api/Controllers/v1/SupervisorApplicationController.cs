using Microsoft.AspNetCore.Mvc;

using InternshipManager.Api.Enums;
using InternshipManager.Api.DTOs.SupervisorApplication;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class SupervisorApplicationController : ControllerBase
{
    private readonly ISupervisorApplicationService _service;
    public SupervisorApplicationController(ISupervisorApplicationService service)
    {
        _service = service;
    }

    [HttpGet("supervisor/{supervisorId:int}")]
    public async Task<IActionResult> GetBySupervisor(
        EmployeeId supervisorId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] SupervisorApplicationStatus? status = null)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { detail = "Некорректные параметры пагинации" });
        
        var (data, totalItems) = await _service
            .GetBySupervisorAsync(supervisorId, page, pageSize, status);
        
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        return Ok(new
        {
            data,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalPages,
                totalItems
            }
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(SupervisorApplicationId id)
    {
        var application = await _service.GetByIdAsync(id);
        
        if (application == null)
            return NotFound(new { detail = "Заявка не найдена" });

        return Ok(application);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        if (page < 1 || pageSize < 1 || pageSize > 100)
            return BadRequest(new { detail = "Некорректные параметры пагинации" });

        var (data, totalItems) = await _service
            .GetActiveAsync(page, pageSize);

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        return Ok(new
        {
            data,
            pagination = new
            {
                currentPage = page,
                pageSize,
                totalPages,
                totalItems
            }
        });
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupervisorApplicationDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.IdSupervisorApplication, version = "1" },
                result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { type = "validation_error", detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { type = "business_error", detail = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        SupervisorApplicationId id,
        [FromBody] UpdateSupervisorApplicationDto dto)
    {
        try
        {
            var result = await _service.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { type = "business_error", detail = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(SupervisorApplicationId id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { type = "business_error", detail = ex.Message });
        }
    }

    [HttpPut("{id:int}/send")]
    public async Task<IActionResult> Send(SupervisorApplicationId id)
    {
        try
        {
            var result = await _service.SendAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { type = "business_error", detail = ex.Message });
        }
    }
    
    [HttpPut("{id:int}/close")]
    public async Task<IActionResult> Close(SupervisorApplicationId id)
    {
        try
        {
            var result = await _service.CloseAsync(id);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { type = "business_error", detail = ex.Message });
        }
    }
}