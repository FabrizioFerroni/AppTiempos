
using System.Text.Json.Serialization;
using static AppTiemposV3.SharedClases.Utilidades.SanitizeText;

namespace AppTiemposV3.SharedClases.DTOs.Requeriments
{
    public enum Tipo
    {
        Informes,
        MigracionPlugins,
        MigracionWeb
    }
    
    public class RequerimentResponseDto
    {
        public Guid Id { get; set; }

        public string ReqID { get; set; } = string.Empty;

        public string Titulo { get; set; } = string.Empty;

        public string Cliente { get; set; } = string.Empty;

        public string StoryPoint { get; set; } = "0.00";

        public string? Url { get; set; } = string.Empty;

        public string? Descripcion { get; set; } = string.Empty;
        
        public Tipo? Tipo { get; set; } = null;
        
        public string? ConjuntoCambios { get; set; } = null;
        
        private int _folderId;
        
        [JsonIgnore]
        public int FolderId
        {
            set => _folderId = value;
        }
        
        public string? FolderPath => _folderId + " - " + ReqID + " - " + SanitizeTitulo(Titulo);

        public UserDto Usuario { get; set; } = new UserDto();
    }

    public class UserDto
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;
    }
}
