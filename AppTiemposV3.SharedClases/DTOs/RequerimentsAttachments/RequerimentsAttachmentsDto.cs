using AppTiemposV3.SharedClases.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.RequerimentsAttachments
{
    public class RequerimentsAttachmentsDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileNameOriginal { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public Etapas Etapa { get; set; }
        public string AttachmentBy { get; set; } = string.Empty;
        public DateTime AttachmentAt { get; set; }
        public Guid RequerimentId { get; set; }
        public string FileBytes { get; set; } = string.Empty;
    }
}
