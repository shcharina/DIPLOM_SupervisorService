using Microsoft.EntityFrameworkCore;
using InternshipManager.Api.Models.Supervisor;

namespace InternshipManager.Api.Data;

public class SupervisorDbContext : DbContext
{
    public SupervisorDbContext(DbContextOptions<SupervisorDbContext> options) : base(options)
    {
    }
    
    public DbSet<SupervisorApplication> SupervisorApplications { get; set; }
    public DbSet<StudentSupervisorApplication> StudentSupervisorApplications { get; set; }
    public DbSet<TimeInterval> TimeIntervals { get; set; }
    public DbSet<InterviewSlot> InterviewSlots { get; set; }
    public DbSet<Interview> Interviews { get; set; }
    public DbSet<SupervisorReview> SupervisorReviews { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // StudentSupervisorApplication - составной ключ уже задан атрибутом [PrimaryKey]
        
        // SupervisorReview - составной ключ уже задан атрибутом [PrimaryKey]
        
        // Interview - 1:1 связь с InterviewSlot
        modelBuilder.Entity<Interview>()
            .HasOne(i => i.InterviewSlot)
            .WithOne(islot => islot.Interview)
            .HasForeignKey<Interview>(i => i.IdInterviewSlot)
            .OnDelete(DeleteBehavior.Cascade);
        
        // InterviewSlot -> TimeInterval
        modelBuilder.Entity<InterviewSlot>()
            .HasOne(islot => islot.TimeInterval)
            .WithMany(ti => ti.InterviewSlots)
            .HasForeignKey(islot => islot.IdInterval)
            .OnDelete(DeleteBehavior.Restrict);
        
        // Индексы для поиска
        modelBuilder.Entity<SupervisorApplication>()
            .HasIndex(sa => sa.Status);
            
        modelBuilder.Entity<SupervisorApplication>()
            .HasIndex(sa => sa.IdEmployee);
            
        modelBuilder.Entity<InterviewSlot>()
            .HasIndex(islot => islot.Status);
            
        modelBuilder.Entity<InterviewSlot>()
            .HasIndex(islot => islot.StartTime);
    }
}