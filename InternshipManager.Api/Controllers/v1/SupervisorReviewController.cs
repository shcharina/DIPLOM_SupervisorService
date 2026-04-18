using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

using InternshipManager.Api.DTOs.SupervisorReview;
using InternshipManager.Api.Services.Interfaces;

namespace InternshipManager.Api.Controllers;

[ApiController]
//[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class SupervisorReviewController : ControllerBase
{
    private readonly ISupervisorReviewService _service;

    public SupervisorReviewController(ISupervisorReviewService service)
    {
        _service = service;
    }

    [HttpGet("pending/{supervisorId:int}")]
    public async Task<IActionResult> GetPendingReviews(EmployeeId supervisorId)
    {
        var result = await _service.GetPendingReviewsAsync(supervisorId);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create( CreateSupervisorReviewDto dto)
    {
        try
        {
            var result = await _service.CreateAsync(dto);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { type = "business_error", detail = ex.Message });
        }
    }

    [HttpGet("{idEmployee:int}/{idStudentApplication:int}")]
    public async Task<IActionResult> GetReview(
        EmployeeId idEmployee,
        StudentApplicationId idStudentApplication)
    {
        var result = await _service.GetReviewAsync(
            idEmployee, idStudentApplication);

        if (result == null)
            return NotFound(new { detail = "Отзыв не найден" });

        return Ok(result);
    }
}