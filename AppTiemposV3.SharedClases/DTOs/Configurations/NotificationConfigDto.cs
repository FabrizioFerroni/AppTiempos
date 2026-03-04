namespace AppTiemposV3.SharedClases.DTOs.Configurations
{
    public class NotificationConfigDto
    {
        public Guid? Id { get; set; }
        public bool EnableNotificationDiario { get; set; } = false;
        public bool EnableNotificationSemanal { get; set; } = false;
        public bool EnableNotificationMetaAlcanzada { get; set; } = false;
        public bool NotificationsEmail { get; set; } = false;
        public string? HoraNotificacionDiaria { get; set; } = string.Empty;
    }
}
