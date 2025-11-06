namespace AppTiemposV3.SharedClases.DTOs;

public class FilterAdvanceDto
{
    public string? SearchTerm { get; set; }
    public string? reqID { get; set; }
    public string? estadoSel { get; set; }
    public DateTime? dateFrom { get; set; }
    public DateTime? dateTo { get; set; }
    public string? timeFrom { get; set; }
    public string? timeTo { get; set; }
}