namespace AppTiemposV3.SharedClases.DTOs.Rejections;

public class RejectionKpiResponse
{
    public int TotalRejections { get; set; }
    public int PendingRejections { get; set; }
    public int InProgressRejections { get; set; }
    public int SuccessRejections { get; set; }
    public int PendingTesting { get; set; }
    public int InProgressTesting { get; set; }
    public int SuccessTesting { get; set; }
}