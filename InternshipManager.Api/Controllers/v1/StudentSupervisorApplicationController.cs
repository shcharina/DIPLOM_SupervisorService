using Microsoft.AspNetCore.Mvc;

using InternshipManager.Api.Enums;
using InternshipManager.Api.DTOs.StudentSupervisorApplication;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class StudentSupervisorApplicationController : ControllerBase
{
    private readonly IStudentSupervisorApplicationService _service;

    public StudentSupervisorApplicationController(
        IStudentSupervisorApplicationService service)
    {
        _service = service;
    }

    [HttpGet("{supervisorApplicationId:int}")]
    public async Task<IActionResult> GetStudents(
        SupervisorApplicationId supervisorApplicationId,
        [FromQuery] StudentSupervisorApplicationStatus? status = null)
    {
        var result = await _service.GetStudentsAsync(supervisorApplicationId, status);
        return Ok(result);
    }

    [HttpGet("student/{studentApplicationId:int}")]
    public async Task<IActionResult> GetByStudent(
        StudentApplicationId studentApplicationId)
    {
        var result = await _service.GetByStudentAsync(studentApplicationId);
        return Ok(result);
    }

    [HttpGet("{supervisorApplicationId:int}/details/{studentApplicationId:int}")]
    public async Task<IActionResult> GetStudentDetails(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        try
        {
            var result = await _service.GetStudentDetailsAsync(
                supervisorApplicationId, studentApplicationId);
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { detail = ex.Message });
        }
    }

    [HttpPost("assign")]
    public async Task<IActionResult> AssignStudent([FromBody] AssignStudentDto dto)
    {
        try
        {
            var result = await _service.AssignStudentAsync(dto);
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

    [HttpPut("{supervisorApplicationId:int}/{studentApplicationId:int}/accept")]
    public async Task<IActionResult> AcceptWithoutInterview(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        try
        {
            var result = await _service.AcceptWithoutInterviewAsync(
                supervisorApplicationId, studentApplicationId);
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

    [HttpPut("{supervisorApplicationId:int}/{studentApplicationId:int}/invite")]
    public async Task<IActionResult> InviteToInterview(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        try
        {
            var result = await _service.InviteToInterviewAsync(
                supervisorApplicationId, studentApplicationId);
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

    [HttpPut("{supervisorApplicationId:int}/{studentApplicationId:int}/reject")]
    public async Task<IActionResult> Reject(
        SupervisorApplicationId supervisorApplicationId,
        StudentApplicationId studentApplicationId)
    {
        try
        {
            var result = await _service.RejectAsync(
                supervisorApplicationId, studentApplicationId);
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