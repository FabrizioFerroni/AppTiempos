using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.SharedClases.DTOs.Requeriments
{
    public class CreateRequerimentDto : RequerimentDto
    {
        [Required]
        [StringLength(15)]
        public required string ReqID { get; set; } = string.Empty;

        [Required]
        public required string Titulo { get; set; } = string.Empty;

        [Required]
        public required string Cliente { get; set; } = string.Empty;

        [Url]
        public string? Url { get; set; } = string.Empty;
        
        
        public string? ConjuntoCambios { get; set; } = string.Empty;
        
        
    }
}
