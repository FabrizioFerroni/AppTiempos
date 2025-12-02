namespace AppTiemposV3.SharedClases.DTOs.Rejections;

public class UpdateRejectionDto
{
    public Guid? Id { get; set; }
    public string? Status { get; set; }
    public bool IsResolve { get; set; } = false;
    public DateTime? ResolvedDate { get; set; }
    public Guid RequerimentId { get; set; }
    public int? TotalRejections { get; set; }
}