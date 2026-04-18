using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using InternshipManager.Api.Enums;
using InternshipManager.Api.DTOs.InterviewSlot;
using InternshipManager.Api.DTOs.TimeInterval;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Controllers;

[ApiController]
//[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class InterviewSlotController : ControllerBase
{
    private readonly IInterviewSlotService _service;

    public InterviewSlotController(IInterviewSlotService service)
    {
        _service = service;
    }

    [HttpGet("pending/{supervisorId:int}")]
    public async Task<IActionResult> GetPending(EmployeeId supervisorId)
    {
        var slots = await _service.GetPendingBySupervisorAsync(supervisorId);
        return Ok(slots);
    }

    [HttpGet("supervisor/{supervisorId:int}")]
    public async Task<IActionResult> GetBySupervisor(EmployeeId supervisorId)
    {
        var slots = await _service.GetBySupervisorAsync(supervisorId);
        return Ok(slots);
    }

    [HttpGet("available/{supervisorId:int}")]
    public async Task<IActionResult> GetAvailable(
        EmployeeId supervisorId,
        [FromQuery] StudentApplicationId? studentApplicationId = null)
    {
        if (!studentApplicationId.HasValue || studentApplicationId <= 0)
            return BadRequest(new { detail = "studentApplicationId обязателен" });

        var slots = await _service.GetAvailableForStudentAsync(
            supervisorId, studentApplicationId.Value);
        return Ok(slots);
    }

    [HttpPut("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(InterviewSlotId id)
    {
        try
        {
            var result = await _service.ConfirmAsync(id);
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

    [HttpPut("{id:int}/reject")]
    public async Task<IActionResult> Reject(InterviewSlotId id, [FromBody] RejectSlotDto dto)
    {
        try
        {
            var result = await _service.RejectAsync(id, dto);
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

    [HttpPut("{id:int}/publish")]
    public async Task<IActionResult> Publish(
        InterviewSlotId id,
        [FromQuery] SupervisorApplicationId? supervisorApplicationId = null)
    {
        try
        {
            var result = await _service.PublishAsync(id, supervisorApplicationId);
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

    [HttpPut("{id:int}/book")]
    public async Task<IActionResult> Book(InterviewSlotId id, [FromBody] BookSlotDto dto)
    {
        try
        {
            var result = await _service.BookAsync(id, dto);
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

    [HttpPost("interval")]
    public async Task<IActionResult> CreateInterval([FromBody] CreateTimeIntervalDto dto)
    {
        try
        {
            var result = await _service.CreateFromIntervalAsync(dto);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { type = "validation_error", detail = ex.Message });
        }
    }
}