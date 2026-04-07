using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.DTOs.Interview;

public class RecordInterviewResultDto
{
    [Required]
    public bool Result { get; set; }      // true = прошёл, false = не прошёл

    [MaxLength(1000)]
    public string? Comment { get; set; }  // комментарий руководителя

}