
using AppTiemposV3.Api.Entities.ConfigurationTable;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace AppTiemposV3.Api.Entities
{
    public class ConfigurationEntity : BaseEntity
    {
        public int ActualConfig { get; set; } = 1;
        public bool AutoBackupEnabled { get; set; }
        public string? BackupFrecuencia { get; set; } = string.Empty;
        public string? BackupTime { get; set; } = string.Empty;
        public int? BackupRetention { get; set; } = 1;
        public int? MaxBackup { get; set; } = 3;

        public Guid WeeklyParId { get; set; }
        [ForeignKey("WeeklyParId")]
        public virtual WeeklyHourConfig WeeklyPar { get; set; } = new();
        public Guid WeeklyImparId { get; set; }
        [ForeignKey("WeeklyImparId")]
        public virtual WeeklyHourConfig WeeklyImpar { get; set; } = new();
        public Guid NotificationConfigId { get; set; }
        [ForeignKey("NotificationConfigId")]
        public virtual NotificationConfigEntity NotificationConfig { get; set; } = new();

        public virtual List<DayConfigEntity> DayConfigs { get; set; } = new();

        public virtual List<WorkingSaturdayEntity> WorkingSaturdays { get; set; } = new();
        public virtual List<BackupLogsEntity> BackupsLogs { get; set; } = new();

        [Required] public required Guid UserId { get; set; }
        public UserEntity User { get; set; } = null!;

    }
}
