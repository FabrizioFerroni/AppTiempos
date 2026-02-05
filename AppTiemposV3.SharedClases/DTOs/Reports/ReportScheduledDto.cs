using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Reports
{
    public class ReportScheduledDto
    {
        public Guid Id { get; set; }
        public string UrlId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Frecuency { get; set; } = string.Empty;
        public List<string>? Destinations { get; set; } = null;
        public List<string>? FullName { get; set; } = null;
        public bool IsScheduled { get; set; } = false;
        public Guid UserId { get; set; }
    }
}
