using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Manager
{
    public class StudentApplication
    {
        [Key]
        public int IdStudentApplication { get; set; }

        // ссылка на тип практики
        public int IdPracticeType { get; set; }
        [ForeignKey(nameof(IdPracticeType))]
        public required StudentApplication PracticeType { get; set; } // Навигационное свойство

        // ссылка на практику
        public int IdScheduledPractice { get; set; }
        [ForeignKey(nameof(IdScheduledPractice))]
        public required StudentApplication ScheduledPractice { get; set; } // Навигационное свойство

        // ссылка на специальность
        public int IdSpecialization { get; set; }
        [ForeignKey(nameof(IdSpecialization))]
        public required StudentApplication Specialization { get; set; } // Навигационное свойство

        public StudentApplicationStatus Status { get; set; }

        public required DateOnly StartDate {  get; set; }
        public required DateOnly EndDate { get; set; }
    }
}
