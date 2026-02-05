using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Reports
{
    public class ScheduleReportDto
    {
        public bool Scheduled { get; set; } = false;
        public string? Frecuency { get; set; } = string.Empty;
        public List<string>? Destinations { get; set; } = null;
    }
}
