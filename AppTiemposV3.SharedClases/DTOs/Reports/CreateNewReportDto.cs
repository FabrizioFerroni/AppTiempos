using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Reports
{
    public class CreateNewReportDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? TableBase { get; set; } = string.Empty;
        public QueryRequestDTO? QueryRequest { get; set; } = null;
        public string ReportMode { get; set; } = string.Empty;
        public string? QueryRaw { get; set; } = string.Empty;
        public ScheduleReportDto Schedule { get; set; } = default!;
        public bool IsScheduled { get; set; } = false;
    }
}
