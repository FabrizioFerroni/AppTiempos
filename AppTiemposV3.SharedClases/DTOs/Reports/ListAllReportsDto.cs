using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Reports
{
    public class ListAllReportsDto
    {
        public Guid Id { get; set; } = Guid.Empty;
        public string Name { get; set; } = string.Empty;
        public string UrlIdentificator { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ReportMode { get; set; } = string.Empty;
        public int JoinsCount { get; set; } = 0;
        public ScheduleReportDto Schedule { get; set; } = default!;
        public QueryRequestDTO? QueryRequest { get; set; } = null;
        public string? QueryRaw { get; set; } = string.Empty;
        public int RunCount { get; set; } = 0;
        public bool IsFavorite { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime LastRun { get; set; }

    }
}
