using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.Models.Shared;

public class Department
{
    [Key]
    public int IdDepartment { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
    
    // Навигационные свойства
    public ICollection<Address> Addresses { get; set; } = new List<Address>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}