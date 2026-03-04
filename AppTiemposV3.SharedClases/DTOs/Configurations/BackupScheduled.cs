namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class BackupScheduled
    {
        public bool AutoBackup { get; set; } = false;
        public string? Frecuencia { get; set; } = string.Empty;
        public string? Time { get; set; } = string.Empty;
        public int? Retention { get; set; } = 1;
        public int? MaxBackup { get; set; } = 3;
    }
}
