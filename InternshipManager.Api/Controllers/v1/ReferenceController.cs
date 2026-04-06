using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Data;
using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Controllers;

[ApiController]
[Asp.Versioning.ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]

public class ReferenceController : ControllerBase
{
    private readonly AppDbContext _context;

    public ReferenceController(AppDbContext context)
    {
        _context = context;
    }

    // GET api/v1/Reference/supervisors

    // Список руководителей (для выбора кто подаёт заявку)

    [HttpGet("supervisors")]

    public async Task<IActionResult> GetSupervisors()
    {
        var supervisors = await _context.Employees
            .Where(e => e.Role == EmployeeRole.Supervisor)
            .Select(e => new
            {
                id = e.IdEmployee,
                fullName = e.LastName + " " + e.FirstName + 
                           (e.Patronymic != null ? " " + e.Patronymic : ""),
                position = e.Position
            })
            .ToListAsync();

        return Ok(supervisors);
    }

    // GET api/v1/Reference/specializations

    [HttpGet("specializations")]

    public async Task<IActionResult> GetSpecializations()
    {
        var specializations = await _context.Specializations
            .Select(s => new
            {
                id = s.IdSpecialization,
                name = s.Name
            })
            .ToListAsync();

        return Ok(specializations);
    }

    // GET api/v1/Reference/departments

    [HttpGet("departments")]

    public async Task<IActionResult> GetDepartments()
    {
        var departments = await _context.Departments
            .Select(d => new
            {
                id = d.IdDepartment,
                name = d.Name
            })
            .ToListAsync();

        return Ok(departments);
    }

    // GET api/v1/Reference/addresses

    // Можно фильтровать по подразделению

    [HttpGet("addresses")]

    public async Task<IActionResult> GetAddresses([FromQuery] int? departmentId = null)
    {
        var query = _context.Addresses.AsQueryable();

        if (departmentId.HasValue)
            query = query.Where(a => a.IdDepartment == departmentId.Value);

        var addresses = await query
            .Select(a => new
            {
                id = a.IdAddress,
                fullAddress = a.FullAddress,
                city = a.City,
                idDepartment = a.IdDepartment
            })
            .ToListAsync();

        return Ok(addresses);
    }

    // GET api/v1/Reference/practice-formats

    // Enum как список

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
        var practices = await _context.ScheduledPractices
            .Select(sp => new
            {
                id = sp.IdScheduledPractice,
                idSpecialization = sp.IdSpecialization,
                startDate = sp.StartDate,
                endDate = sp.EndDate
            })
            .ToListAsync();

        return Ok(practices);
    }

}