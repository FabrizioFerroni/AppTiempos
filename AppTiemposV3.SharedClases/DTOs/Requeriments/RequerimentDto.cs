
namespace AppTiemposV3.SharedClases.DTOs.Requeriments
{
    public class RequerimentDto
    {
        public string ReqID { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string StoryPoint { get; set; } = "0.00";
        public string? Url { get; set; } = string.Empty;
        public string? Descripcion { get; set; } = string.Empty;
        public Guid UserId { get; set; }
    }
}
