namespace AppTiemposV3.Api.Entities
{
    public class TesterEntity : BaseEntity
    {
        public string Nombre { get; set; } = string.Empty;
        public bool Activo { get; set; } = true;
        public ICollection<RejectionEntity>? Rejections { get; set; }
        public ICollection<RejectionDetailEntity>? RejectionDetails { get; set; }
    }
}
