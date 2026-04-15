using Microsoft.EntityFrameworkCore;
using InternshipManager.Api.Data;
using InternshipManager.Api.Models.Supervisor;
using InternshipManager.Api.Repositories.Interfaces;

namespace InternshipManager.Api.Repositories;

public class SupervisorReviewRepository : ISupervisorReviewRepository
{
    private readonly AppDbContext _context;

    public SupervisorReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SupervisorReview?> GetAsync(
        EmployeeId idEmployee,
        StudentApplicationId idStudentApplication)
    {
        return await _context.SupervisorReviews
            .AsNoTracking()
            .FirstOrDefaultAsync(r =>
                r.IdEmployee == idEmployee &&
                r.IdStudentApplication == idStudentApplication);
    }

    public async Task<bool> ExistsAsync(
        EmployeeId idEmployee,
        StudentApplicationId idStudentApplication)
    {
        return await _context.SupervisorReviews
            .AsNoTracking()
            .AnyAsync(r =>
                r.IdEmployee == idEmployee &&
                r.IdStudentApplication == idStudentApplication);
    }

    public async Task<List<StudentApplicationId>> GetReviewedStudentIdsAsync(
        EmployeeId supervisorId)
    {
        return await _context.SupervisorReviews
            .AsNoTracking()
            .Where(r => r.IdEmployee == supervisorId)
            .Select(r => r.IdStudentApplication)
            .ToListAsync();
    }

    public async Task AddAsync(SupervisorReview review)
    {
        _context.SupervisorReviews.Add(review);
        await _context.SaveChangesAsync();
    }
}