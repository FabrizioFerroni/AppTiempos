using static NanoidDotNet.Nanoid;

namespace AppTiemposV3.Web.Utils;


public enum NotificationType { Success, Info, Warning, Error }

public enum NotificationPosition
{
    TopLeft,
    TopCenter,
    TopRight,
    BottomLeft,
    BottomCenter,
    BottomRight
}

public class Notification
{
    public string Id { get; set; } = Generate(Alphabets.LowercaseLettersAndDigits, 10);
    public NotificationType Type { get; set; } = NotificationType.Info;
    public NotificationPosition Position { get; set; } = NotificationPosition.TopRight;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Icon { get; set; }

    public bool IsFixed { get; set; } = false;
    public int Duration { get; set; } = 5000; // ms
    public bool ShowProgress { get; set; } = true;
    
    public int Progress { get; set; } = 100; // de 100 a 0
    
    public bool IsVisible { get; set; } = true; // para animaciones
    
    public bool HasAnimated { get; set; } = false;
}