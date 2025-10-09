using AppTiemposV3.SharedClases.Enums;

namespace AppTiemposV3.SharedClases.DTOs.Requeriments
{
    public class UpdateRequerimentDto
    {
        public Guid Id { get; set; }
        public string? ReqID { get; set; }
        public string? Titulo { get; set; }
        public string? Cliente { get; set; }
        public string? StoryPoint { get; set; }
        public string? Url { get; set; }
        public string? Descripcion { get; set; }
        public Guid CategoryId { get; set; }
        public string? ConjuntoCambios { get; set; }
        
        public Estados? Estado  { get; set; }
        public Etapas? EtapaActual { get; set; }
    }
}
