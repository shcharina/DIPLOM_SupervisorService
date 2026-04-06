using InternshipManager.Api.Enums;

using System.ComponentModel.DataAnnotations;

namespace InternshipManager.Api.DTOs.SupervisorApplication;

public class UpdateSupervisorApplicationDto

{

    public int? IdSpecialization { get; set; }

    public int? IdDepartment { get; set; }

    public int? IdAddress { get; set; }

    public int? IdScheduledPractice { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    [Range(1, 100)]

    public int? RequestedStudentsCount { get; set; }

    public PracticeFormat? PracticeFormat { get; set; }

    public bool? IsPaid { get; set; }

}

