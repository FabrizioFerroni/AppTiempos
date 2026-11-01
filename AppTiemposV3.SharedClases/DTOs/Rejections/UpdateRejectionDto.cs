namespace AppTiemposV3.SharedClases.DTOs.Rejections;

public class UpdateRejectionDto
{
    public Guid? Id { get; set; }
    public string? Status { get; set; }
    public bool IsResolve { get; set; } = false;
    public DateTime? ResolvedDate { get; set; }
    public Guid RequerimentId { get; set; }
    public int? TotalRejections { get; set; }
    public Guid TesterId { get; set; } = Guid.Empty;
    public bool IsResolveTesting { get; set; } = false;
    public DateTime? ResolvedDateTesting { get; set; } = null;
    public string? StatusTesting { get; set; } = "pending";
}