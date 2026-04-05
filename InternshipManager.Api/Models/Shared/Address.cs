using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManager.Api.Models.Shared;

public class Address
{
    [Key]
    public int IdAddress { get; set; }
    
    [Required]
    public int IdDepartment { get; set; }
    
    [Required]
    [MaxLength(255)]
    public string FullAddress { get; set; } = string.Empty;
    
    [MaxLength(255)]
    public string? City { get; set; }
    
    [MaxLength(255)]
    public string? PostalCode { get; set; }
    
    // Навигационные свойства
    [ForeignKey(nameof(IdDepartment))]
    public Department Department { get; set; } = null!;
}