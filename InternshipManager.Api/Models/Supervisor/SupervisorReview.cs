using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InternshipManager.Api.Models.Supervisor;

[PrimaryKey(nameof(IdEmployee), nameof(IdStudentApplication))]
public class SupervisorReview
{
    public EmployeeId IdEmployee { get; set; }  // Руководитель
    
    public StudentApplicationId IdStudentApplication { get; set; }  // Заявка студента
    
    public bool RecommendedForEmployment { get; set; } = false;
    
    [Range(1, 5)]
    public int PvScore { get; set; }  // Выполнение ПВР
    
    [Range(1, 5)]
    public int SkillsScore { get; set; }  // Освоение навыков
    
    [Range(1, 5)]
    public int IndependenceScore { get; set; }  // Самостоятельность
    
    [Range(1, 5)]
    public int TeamworkScore { get; set; }  // Работа в команде
    
    [Range(1, 5)]
    public int OverallScore { get; set; }  // Общая оценка
    
    public string? Comment { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}