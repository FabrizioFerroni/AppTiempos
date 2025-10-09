using AppTiemposV3.Web.Components.Icons;
using AppTiemposV3.Web.Services;
using AppTiemposV3.Web.Utils;
using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class NotificationContainer : ComponentBase,  IDisposable
{
    [Inject] private NotificationService NotificationService { get; set; } = default!;

    private readonly Dictionary<NotificationPosition, List<Notification>> notificationsByPosition
        = Enum.GetValues(typeof(NotificationPosition))
              .Cast<NotificationPosition>()
              .ToDictionary(p => p, _ => new List<Notification>());

    protected override void OnInitialized()
    {
        NotificationService.OnNotify += AddNotification;
    }

    private void AddNotification(Notification n)
    {
        n.IsVisible = true;
        n.Progress = 100;

        notificationsByPosition[n.Position].Add(n);
        StateHasChanged();

        if (!n.IsFixed)
        {
            _ = StartTimer(n);
        }
    }

    private async Task StartTimer(Notification n)
    {
        int interval = 100;
        int steps = n.Duration / interval;

        for (int i = 0; i < steps; i++)
        {
            n.Progress = 100 - (i * 100 / steps);
            StateHasChanged();
            await Task.Delay(interval);
        }

        n.IsVisible = false;
        StateHasChanged();

        await Task.Delay(500);

        notificationsByPosition[n.Position].Remove(n);
        StateHasChanged();
    }

    

    private void Close(string id)
    {
        foreach (KeyValuePair<NotificationPosition, List<Notification>> kv in notificationsByPosition)
        {
            Notification? n = kv.Value.FirstOrDefault(x => x.Id == id);
            if (n != null)
            {
                n.IsVisible = false;
                StateHasChanged();
                _ = Task.Run(async () =>
                {
                    await Task.Delay(500);
                    kv.Value.Remove(n);
                    StateHasChanged();
                });
                break;
            }
        }
    }

    private string GetContainerClass(NotificationPosition pos) => pos switch
    {
        NotificationPosition.TopLeft => "fixed top-5 left-5 z-[9999]",
        NotificationPosition.TopCenter => "fixed top-5 left-1/2 -translate-x-1/2 z-[9999]",
        NotificationPosition.TopRight => "fixed top-5 right-5 z-[9999]",
        NotificationPosition.BottomLeft => "fixed bottom-5 left-5 z-[9999]",
        NotificationPosition.BottomCenter => "fixed bottom-5 left-1/2 -translate-x-1/2 z-[9999]",
        NotificationPosition.BottomRight => "fixed bottom-5 right-5 z-[9999]",
        _ => "fixed top-5 right-5 z-[9999]"
    };

    private string GetTypeClasses(NotificationType type) => type switch
    {
        NotificationType.Success => "border-green-500",
        NotificationType.Info    => "border-blue-500",
        NotificationType.Warning => "border-yellow-500",
        NotificationType.Error   => "border-red-500",
        _ => "border-gray-500"
    };
    
    private string GetClasses(NotificationType type, bool isVisible)
    {
        string baseClasses =
            "relative bg-white dark:bg-gray-800 border-l-4 shadow-lg rounded-lg select-none p-4 mb-3 transition-all duration-500 ease-in-out";

        string isVisibleClasses = isVisible ? "opacity-100 translate-x-0 animate-tada" : "opacity-0 translate-x-5";
        
        string typeClasses = GetTypeClasses(type);

        return Cn(baseClasses, isVisibleClasses, typeClasses);
    }

    private RenderFragment GetNotificationIcon(NotificationType type) => builder =>
    {
        switch (type)
        {
            case NotificationType.Success:
                builder.OpenComponent<CheckCircle>(0);
                builder.AddAttribute(1, "Class", "h-5 w-5 text-green-500");
                builder.CloseComponent();
                break;
            case NotificationType.Warning:
                builder.OpenComponent<AlertCircle>(0);
                builder.AddAttribute(1, "Class", "h-5 w-5 text-yellow-500");
                builder.CloseComponent();
                break;
            case NotificationType.Error:
                builder.OpenComponent<CloseCircle>(0);
                builder.AddAttribute(1, "Class", "h-5 w-5 text-red-500");
                builder.CloseComponent();
                break;
            default:
                builder.OpenComponent<InfoCircle>(0);
                builder.AddAttribute(1, "Class", "h-5 w-5 text-blue-500");
                builder.CloseComponent();
                break;
        }
    };

    public void Dispose()
    {
        NotificationService.OnNotify -= AddNotification;
    }
}