using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata;

using InternshipManager.Api.Models.Shared;

namespace InternshipManager.Api.Models.Manager
{
    [PrimaryKey(nameof(IdDocumentType), nameof(IdSpecialization))]
    public class DocumentForSpecialization
    {
        // [Key]
        public int IdDocumentType { get; set; }

        // [Key]
        public int IdSpecialization { get; set; }

        public ICollection<StudentDocument> StudentDocument { get; set; } = new List<StudentDocument>(); // Навигационное свойство
    }
}
