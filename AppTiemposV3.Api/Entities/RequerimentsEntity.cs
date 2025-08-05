using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppTiemposV3.Api.Entities
{
    
    public enum Tipo
    {
        Informes,
        MigracionPlugins,
        MigracionWeb
    }
    
    public class RequerimentsEntity : BaseEntity
    {
        [Required]
        [StringLength(15)]
        public required string ReqID { get; set; } = string.Empty;

        [Required]
        public required string Titulo { get; set; } = string.Empty;

        [Required]
        public required string Cliente { get; set; } = string.Empty;

        public string StoryPoint { get; set; } = "0.00";

        [Url]
        public string? Url { get; set; } = null;

        public string? Descripcion {  get; set; } = null;

        [Required]
        public required Guid UserId { get; set; }

        public UserEntity User { get; set; } = null!;
        
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int FolderId { get; set; }
        
        public Tipo? Tipo {get; set;}
        
        public string? ConjuntoCambios { get; set; } = null;
        
        public ICollection<ActivitiesEntity> Activities { get; set; } = new List<ActivitiesEntity>();
        
        public ICollection<TrainingEntity> Trainings { get; set; } = new List<TrainingEntity>();

    }
}
