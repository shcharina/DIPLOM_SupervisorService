using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.DTOs.SupervisorReview;

public class CreateSupervisorReviewDto
{
    [Required]
    public Guid IdEmployee { get; set; }

    [Required]
    public Guid IdStudentApplication { get; set; }

    public bool RecommendedForEmployment { get; set; } = false;

    [Required]
    [Range(1, 5)]
    public int PvScore { get; set; }           // Выполнение ПВР

    [Required]
    [Range(1, 5)]
    public int SkillsScore { get; set; }       // Освоение навыков

    [Required]
    [Range(1, 5)]
    public int IndependenceScore { get; set; } // Самостоятельность

    [Required]
    [Range(1, 5)]
    public int TeamworkScore { get; set; }     // Работа в команде

    [Required]
    [Range(1, 5)]
    public int OverallScore { get; set; }      // Общая оценка

    [MaxLength(2000)]
    public string? Comment { get; set; }

}