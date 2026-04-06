using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

using InternshipManager.Api.Enums;

namespace InternshipManager.Api.Models.Manager
{
    public class StudentDocument 
    {
        [Key]
        public int IdDocument { get; set; }

        // ссылка на заявку студента
        public int IdStudentApplication { get; set; }
        [ForeignKey(nameof(StudentApplication))]
        public required StudentApplication SupervisorApplication { get; set; } // Навигационное свойство

        // ссылка на документ для специальности (2 внешних ключа)
        public int IdDocumentType { get; set; }
        public int IdSpecialization { get; set; }
        public required DocumentForSpecialization DocumentForSpecialization { get; set; } // Навигационное свойство

        public bool isLoaded { get; set; }

        public DocumentCheckStatus DocumentCheckStatus { get; set; }

        public DateOnly LoadDate {  get; set; }

        [MaxLength(255)]
        public string PathToFile { get; set; } = string.Empty;

        public SigningDocumentStatus SigningDocumentStatus { get; set; }
    }
}
