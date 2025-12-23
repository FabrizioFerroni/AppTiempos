namespace AppTiemposV3.SharedClases.DTOs.Audits;

public class AuditKpiResponse
{
    public int TotalAudits { get; set; }
    public int ActivityData { get; set; }
    public int TrainingData { get; set; }
    public int RequerimentData { get; set; }
    public int RejectionData { get; set; }
}