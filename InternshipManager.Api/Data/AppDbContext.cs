using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

                // Глобальная настройка для всех DateTime свойств
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime))
                {
                    property.SetValueConverter(new UtcDateTimeConverter());
                }
                else if (property.ClrType == typeof(DateTime?))
                {
                    property.SetValueConverter(new UtcNullableDateTimeConverter());
                }
            }
        }
        // Составной ключ для StudentSupervisorApplication
        modelBuilder.Entity<StudentSupervisorApplication>()
            .HasKey(x => new { x.IdSupervisorApplication, x.IdStudentApplication });

        // Составной ключ для SupervisorReview
        modelBuilder.Entity<SupervisorReview>()
            .HasKey(x => new { x.IdEmployee, x.IdStudentApplication });
    }
}

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter() 
        : base(
            v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc)
        )
    { }
}

public class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter() 
        : base(
            v => v.HasValue && v.Value.Kind != DateTimeKind.Utc ? v.Value.ToUniversalTime() : v,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : v
        )
    { }
}