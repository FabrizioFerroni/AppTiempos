namespace AppTiemposV3.SharedClases.DTOs.Audits;

public class AuditFilterDto
{
    public string? ActivityActionSearch { get; set; }
    public string? EntitySearch { get; set; }
    public string? ActionSearch { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}