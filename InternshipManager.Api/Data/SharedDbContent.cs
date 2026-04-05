using Microsoft.EntityFrameworkCore;
using InternshipManager.Api.Models.Shared;

namespace InternshipManager.Api.Data;

public class SharedDbContext : DbContext
{
    public SharedDbContext(DbContextOptions<SharedDbContext> options) : base(options)
    {
    }
    
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Specialization> Specializations { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<PracticeType> PracticeTypes { get; set; }
    public DbSet<ScheduledPractice> ScheduledPractices { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Уникальные индексы
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Email)
            .IsUnique();
            
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.Login)
            .IsUnique();
            
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.PersonnelNumber)
            .IsUnique();
        
        // Связи
        modelBuilder.Entity<Address>()
            .HasOne(a => a.Department)
            .WithMany(d => d.Addresses)
            .HasForeignKey(a => a.IdDepartment)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<Employee>()
            .HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.IdDepartment)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<ScheduledPractice>()
            .HasOne(sp => sp.Specialization)
            .WithMany(s => s.ScheduledPractices)
            .HasForeignKey(sp => sp.IdSpecialization)
            .OnDelete(DeleteBehavior.Restrict);
            
        modelBuilder.Entity<ScheduledPractice>()
            .HasOne(sp => sp.PracticeType)
            .WithMany(pt => pt.ScheduledPractices)
            .HasForeignKey(sp => sp.IdPracticeType)
            .OnDelete(DeleteBehavior.Restrict);
    }
}