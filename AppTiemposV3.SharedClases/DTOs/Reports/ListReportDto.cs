namespace AppTiemposV3.SharedClases.DTOs.Reports
{
    public class ListReportDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public string UrlIdentificator { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReportMode { get; set; } = string.Empty;
        public List<Dictionary<string, object?>> DataResult { get; set; } = default!;
        public int QueryResult { get; set; } = 0;
        public DateTime CreatedAt { get; set; }
    }
}
