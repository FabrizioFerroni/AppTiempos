namespace AppTiemposV3.Api.Entities.ConfigurationTable
{
    public class NotificationConfigEntity : BaseEntity
    {
        public bool EnableNotificationDiario { get; set; } = false;
        public bool EnableNotificationSemanal { get; set; } = false;
        public bool EnableNotificationMetaAlcanzada { get; set; } = false;
        public bool NotificationsEmail { get; set; } = false;
        public string? HoraNotificacionDiaria { get; set; } = string.Empty;
    }
}
