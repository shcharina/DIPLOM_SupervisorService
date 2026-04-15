using Microsoft.AspNetCore.Mvc;

using InternshipManager.Api.DTOs.Interview;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class InterviewController : ControllerBase
{
    private readonly IInterviewService _service;

    public InterviewController(IInterviewService service)
    {
        _service = service;
    }

    [HttpGet("supervisor/{supervisorId}")]
    public async Task<IActionResult> GetBySupervisor(EmployeeId supervisorId)
    {
        var result = await _service.GetBySupervisorAsync(supervisorId);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(InterviewSlotId id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null)
            return NotFound(new { detail = "Собеседование не найдено" });
        return Ok(result);
    }

    [HttpPut("{id:int}/result")]
    public async Task<IActionResult> RecordResult(
        InterviewSlotId id, RecordInterviewResultDto dto)
    {
        try
        {
            var result = await _service.RecordResultAsync(id, dto);
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