using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.DTOs.TimeInterval;

public class CreateTimeIntervalDto
{
    [Required]
    public Guid IdEmployee { get; set; }     // для кого интервал

    public Guid? IdCreator { get; set; }      // кто создаёт (null = сам руководитель)

    [Required]
    public DateTime StartTime { get; set; }

    [Required]
    public DateTime EndTime { get; set; }

    [Range(1, 20)]
    public int MaxCount { get; set; } = 1;

    public TimeSpan? BreakDuration { get; set; }

}