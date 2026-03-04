using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class SaturdayBannerConfigDto
    {
        public bool SaturdayWork { get; set; }
        public string SemanaWork { get; set; } = string.Empty;
        public string DayWork { get; set; } = string.Empty;
        public string HorasWork { get; set; } = string.Empty;
        public double TotalHsWork { get; set; }
    }
}
