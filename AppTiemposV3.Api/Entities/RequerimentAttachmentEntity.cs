using AppTiemposV3.SharedClases.Enums;
using System.ComponentModel.DataAnnotations;

namespace AppTiemposV3.Api.Entities
{
    public class RequerimentAttachmentEntity : BaseEntity
    {
        [Required]
        public string Descripcion { get; set; } = String.Empty;
        [Required]
        public string AttachmentBy { get; set; } = String.Empty;
        public string FileName { get; set; } = string.Empty;
        public string FileNameOriginal { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public DateTime AttachmentAt { get; set; } = DateTime.UtcNow;
        public Etapas Etapa { get; set; } = Etapas.Alta;
        [Required]
        public required Guid RequerimentId { get; set; }
        public RequerimentsEntity Requeriments { get; set; } = default!;

        [Required]
        public required Guid UserId { get; set; }

        public UserEntity User { get; set; } = null!;
    }
}
