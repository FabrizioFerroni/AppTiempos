using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class UpdateConfigDto
    {
        public List<DayConfig> DayConfigs { get; set; } = new List<DayConfig>();
        public DayHours WeeklyPar { get; set; } = new DayHours();
        public DayHours WeeklyImpar { get; set; } = new DayHours();
        public List<WorkingSaturday> WorkingSaturdays { get; set; } = new List<WorkingSaturday>();
        public NotificationConfigDto NotificationConfig { get; set; } = new NotificationConfigDto();
        public BackupScheduled BackupScheduled { get; set; } = new BackupScheduled();
        public List<Guid> WorkingSaturdayToDelete { get; set; } = new List<Guid>();
    }
}
