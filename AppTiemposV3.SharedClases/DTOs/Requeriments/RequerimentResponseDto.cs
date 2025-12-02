
using System.Text.Json.Serialization;
using AppTiemposV3.SharedClases.Enums;
using static AppTiemposV3.SharedClases.Utilidades.SanitizeText;

namespace AppTiemposV3.SharedClases.DTOs.Requeriments
{
    public class RequerimentResponseDto
    {
        public Guid Id { get; set; }

        public string ReqID { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public string Cliente { get; set; } = string.Empty;

        public string StoryPoint { get; set; } = "0.00";

        public string? Url { get; set; } = string.Empty;

        public string? Descripcion { get; set; } = string.Empty;
        
        public Estados Estado { get; set; } = Estados.Pendiente;

        public Etapas Etapa { get; set; } = Etapas.Alta;
        
        public Etapas EtapaActual { get; set; } = Etapas.Alta;
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ConjuntoCambios { get; set; } = null;
        
        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }
        
        private int _folderId;
        
        //[JsonIgnore]
        public int FolderId
        {
            get => _folderId;
            set => _folderId = value;
        }
        
        public string? FolderPath => _folderId + " - " + ReqID + " - " + SanitizeTitulo(Titulo);

        public UserDto Usuario { get; set; } = new UserDto();
        
        public CategoryDtoA Category { get; set; } = new CategoryDtoA();
        
        public TimeSpan WorkedTime { get; set; }
        
       public int TotalRejections { get; set; } = 0;
    }

    public class UserDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;
    }
    
    public class CategoryDtoA
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    
        public string Color { get; set; } = string.Empty;
    }
}
