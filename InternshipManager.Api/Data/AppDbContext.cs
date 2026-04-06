using InternshipManager.Api.Enums;
using InternshipManager.Api.Models.Shared;
using InternshipManager.Api.Models.Supervisor;
using Microsoft.EntityFrameworkCore;
namespace InternshipManager.Api.Data;
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    // Shared таблицы (общая БД)
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<PracticeType> PracticeTypes { get; set; }
    public DbSet<ScheduledPractice> ScheduledPractices { get; set; }

    // Supervisor таблицы (твоя часть)
    public DbSet<SupervisorApplication> SupervisorApplications { get; set; }
    public DbSet<StudentSupervisorApplication> StudentSupervisorApplications { get; set; }
    public DbSet<InterviewSlot> InterviewSlots { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<TimeInterval> TimeIntervals { get; set; }
    public DbSet<SupervisorReview> SupervisorReviews { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Shared таблицы — исключаем из миграций
        modelBuilder.Entity<Employee>()
            .ToTable("Employees", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<Department>()
            .ToTable("Departments", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<Address>()
            .ToTable("Addresses", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<Specialization>()
            .ToTable("Specializations", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<PracticeType>()
            .ToTable("PracticeTypes", t => t.ExcludeFromMigrations());
        modelBuilder.Entity<ScheduledPractice>()
            .ToTable("ScheduledPractices", t => t.ExcludeFromMigrations());

        // Составной ключ для StudentSupervisorApplication
        modelBuilder.Entity<StudentSupervisorApplication>()
            .HasKey(x => new { x.IdSupervisorApplication, x.IdStudentApplication });

        // Составной ключ для SupervisorReview
        modelBuilder.Entity<SupervisorReview>()
            .HasKey(x => new { x.IdEmployee, x.IdStudentApplication });

    }

}
