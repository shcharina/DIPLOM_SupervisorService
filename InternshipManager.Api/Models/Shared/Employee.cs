using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Shared;

public class Employee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // убрать если GUid
    public EmployeeId IdEmployee { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(255)]
    public string LastName { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? Patronymic { get; set; }
    
    [MaxLength(255)]
    public string? PersonnelNumber { get; set; }
    
    [MaxLength(255)]
    public string Position { get; set; } = string.Empty;
    
    public DepartmentId IdDepartment { get; set; }
    
    public EmployeeRole Role { get; set; } = EmployeeRole.Supervisor;
    
    [MaxLength(255)]
    public string Phone { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string Login { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string PasswordHash { get; set; } = string.Empty;
    
    // Навигационные свойства
    [ForeignKey(nameof(IdDepartment))]
    public Department? Department { get; set; }
}