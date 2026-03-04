
namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class ListActualConfig
    {
        public Guid? Id { get; set; }
        public int ActualConfig { get; set; }
        public List<DayConfig> DayConfigs { get; set; } = new List<DayConfig>();
        public DayHours WeeklyPar { get; set; } = new DayHours();
        public DayHours WeeklyImpar { get; set; } = new DayHours();
        public List<WorkingSaturday> WorkingSaturdays { get; set; } = new List<WorkingSaturday>();
        public NotificationConfigDto NotificationConfig { get; set; } = new NotificationConfigDto();
        public BackupScheduled BackupScheduled { get; set; } = new BackupScheduled();
        public UserDtoConfig User { get; set; } = new UserDtoConfig();
        public bool IsNotUpdated { get; set; } = false;
    }

    public class UserDtoConfig
    {
        public Guid? Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Area { get; set; } = string.Empty;
    }
}
