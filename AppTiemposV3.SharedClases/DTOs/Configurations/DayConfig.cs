using System.Text.Json.Serialization;
using static AppTiemposV3.SharedClases.Utilidades.DateHelper;

namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class DayConfig
    {
        public Guid? Id { get; set; }
        public DiasSemana Day { get; set; }
        public string DayName { get; set; } = string.Empty;
        public double MinHours { get; set; }
        public double MaxHours { get; set; }
        public bool Enabled { get; set; }
    }
}
