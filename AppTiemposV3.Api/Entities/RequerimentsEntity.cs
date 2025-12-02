using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.Api.Entities
{
    public class RequerimentsEntity : BaseEntity
    {
        [Required]
        [StringLength(15)]
        [RegularExpression(@"^\d+$", ErrorMessage = "Solo se permiten números")]
        public required string ReqID { get; set; } = string.Empty;

        [Required]
        public required string Titulo { get; set; } = string.Empty;

        [Required]
        public required string Cliente { get; set; } = string.Empty;

        [RegularExpression(@"^[0-9\.]$", ErrorMessage = "Solo se permiten números")]
        [Required]
        public required string StoryPoint { get; set; } = string.Empty;

        [Url]
        public string? Url { get; set; } = null;

        public string? Descripcion {  get; set; } = null;

        [Required]
        public required Guid UserId { get; set; }

        public UserEntity User { get; set; } = null!;
        
        public int FolderId { get; set; }

        public Estados Estado { get; set; } = Estados.Pendiente;
        
        public Etapas EtapaActual { get; set; } = Etapas.Alta;
        
        public string? ConjuntoCambios { get; set; } = null;
        
        [NotMapped]
        public TimeSpan WorkedTime { get; set; }
        
        public ICollection<ActivitiesEntity> Activities { get; set; } = new List<ActivitiesEntity>();
        
        public ICollection<TrainingEntity> Trainings { get; set; } = new List<TrainingEntity>();
        
        public ICollection<RejectionEntity> Rejections { get; set; } = new List<RejectionEntity>();
        
        [Required]
        public required Guid CategoryId { get; set; } = Guid.Empty;
    
        public CategoriesEntity Category { get; set; } = null!;

    }
}
