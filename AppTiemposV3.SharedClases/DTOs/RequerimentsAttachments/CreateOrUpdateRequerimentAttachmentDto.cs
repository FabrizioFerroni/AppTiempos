using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments
{
    public class CreateOrUpdateRequerimentAttachmentDto
    {
        public Etapas Etapa { get; set; }
        public string AttachmentBy { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public DateTime AttachmentAt { get; set; }
        public Guid RequerimentId { get; set; }
    }
}
