namespace AppTiemposV3.SharedClases.GenericModels;

public enum NotificationType
{
    Info,    // 0
    Success,   // 1
    Warning,      // 2
    Error      // 3
}

public class NotificationModel
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; } = NotificationType.Info;
    public DateTime Date { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;
    public bool IsDelete { get; set; } = false;
    public string? ActionUrl { get; set; }
}