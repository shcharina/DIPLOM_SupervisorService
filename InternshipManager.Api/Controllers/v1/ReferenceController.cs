using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using InternshipManager.Api.Services;

public class ReferenceController : ControllerBase
{
    private readonly ManagerApiClient _managerApi;
    public ReferenceController(ManagerApiClient managerApi)
    {
        _managerApi = managerApi;
    }

    // GET api/v1/Reference/specializations

    [HttpGet("specializations")]

    public async Task<IActionResult> GetSpecializations()
    {
        var specializations = await _managerApi.GetSpecializationsAsync();
        return Ok(specializations);
    }

    // GET api/v1/Reference/departments

    [HttpGet("departments")]

    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _managerApi.GetDepartmentsAsync();
        return Ok(departments);
    }

    // GET api/v1/Reference/addresses

    [HttpGet("addresses")]
    public async Task<IActionResult> GetAddresses(
        [FromQuery] int? departmentId = null)
    {
        var addresses = await _managerApi.GetAddressesAsync(departmentId);
        return Ok(addresses);
    }

    // GET api/v1/Reference/practice-formats

    [HttpGet("practice-formats")]

    public IActionResult GetPracticeFormats()
    {
        var formats = new[]
        {
            new { id = 1, name = "Очная" },
            new { id = 2, name = "Дистанционная" },
            new { id = 3, name = "Гибридная" }
        };
        return Ok(formats);
    }

    // GET api/v1/Reference/scheduled-practices

    [HttpGet("scheduled-practices")]

    public async Task<IActionResult> GetScheduledPractices()
    {
        var practices = await _managerApi.GetScheduledPracticesAsync();
        return Ok(practices);
    }
}
