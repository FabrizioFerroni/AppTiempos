using AppTiemposV3.Web.Utils;

namespace AppTiemposV3.Web.Services;

public class NotificationService
{
    public event Action<Notification>? OnNotify;

    public void Show(Notification notification) => OnNotify?.Invoke(notification);

    public void Success(string title, string? description = null,
        int duration = 5000,
        NotificationPosition position = NotificationPosition.TopRight,
        bool isFixed = false,
        bool showProgress = true,
        string? icon = null)
    {
        Show(new Notification {
            Type = NotificationType.Success,
            Title = title,
            Description = description,
            Duration = duration,
            Position = position,
            IsFixed = isFixed,
            ShowProgress = showProgress,
            Icon = icon
        });
    }

    public void Info(string title, string? description = null,
        int duration = 5000,
        NotificationPosition position = NotificationPosition.TopRight,
        bool isFixed = false,
        bool showProgress = true,
        string? icon = null)
    {
        Show(new Notification {
            Type = NotificationType.Info,
            Title = title,
            Description = description,
            Duration = duration,
            Position = position,
            IsFixed = isFixed,
            ShowProgress = showProgress,
            Icon = icon
        });
    }
    
    public void Warning(string title, string? description = null,
        int duration = 5000,
        NotificationPosition position = NotificationPosition.TopRight,
        bool isFixed = false,
        bool showProgress = true,
        string? icon = null)
    {
        Show(new Notification {
            Type = NotificationType.Warning,
            Title = title,
            Description = description,
            Duration = duration,
            Position = position,
            IsFixed = isFixed,
            ShowProgress = showProgress,
            Icon = icon
        });
    }
    public void Error(string title, string? description = null,
        int duration = 5000,
        NotificationPosition position = NotificationPosition.TopRight,
        bool isFixed = false,
        bool showProgress = true,
        string? icon = null)
    {
        Show(new Notification {
            Type = NotificationType.Error,
            Title = title,
            Description = description,
            Duration = duration,
            Position = position,
            IsFixed = isFixed,
            ShowProgress = showProgress,
            Icon = icon
        });
    }
}