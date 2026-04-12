using Microsoft.EntityFrameworkCore;

using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Supervisor таблицы
    public DbSet<SupervisorApplication> SupervisorApplications { get; set; }
    public DbSet<StudentSupervisorApplication> StudentSupervisorApplications { get; set; }
    public DbSet<InterviewSlot> InterviewSlots { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<TimeInterval> TimeIntervals { get; set; }
    public DbSet<SupervisorReview> SupervisorReviews { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Составной ключ для StudentSupervisorApplication
        modelBuilder.Entity<StudentSupervisorApplication>()
            .HasKey(x => new { x.IdSupervisorApplication, x.IdStudentApplication });

        // Составной ключ для SupervisorReview
        modelBuilder.Entity<SupervisorReview>()
            .HasKey(x => new { x.IdEmployee, x.IdStudentApplication });
    }

}