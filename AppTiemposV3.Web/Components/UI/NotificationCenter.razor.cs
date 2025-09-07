using AppTiemposV3.SharedClases.GenericModels;
using AppTiemposV3.Web.Components.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static NanoidDotNet.Nanoid;

namespace AppTiemposV3.Web.Components.UI;

public partial class NotificationCenter : ComponentBase
{
    [Inject] private IJSRuntime Js { get; set; } = null!;
    private List<NotificationModel> Notifications = new()
    {
        new NotificationModel
        {
            Id = Generate(Alphabets.LowercaseLettersAndDigits, 10), 
            Title = "Nuevo mensaje", 
            Message = "Tienes un mensaje nuevo", 
            Type = NotificationType.Info, 
            Date = DateTime.Now.AddMinutes(-10), 
            IsRead = false,
            IsDelete = false
        },
        new NotificationModel
        {
            Id = Generate(Alphabets.LowercaseLettersAndDigits, 10), 
            Title = "Test 2", 
            Message = "Test 2", 
            Type = NotificationType.Success, 
            Date = DateTime.Now.AddHours(-7), 
            IsRead = false,
            IsDelete = false
        },
        new NotificationModel
        {
            Id = Generate(Alphabets.LowercaseLettersAndDigits, 10), 
            Title = "Actualización", 
            Message = "El sistema se actualizará mañana", 
            Type = NotificationType.Warning, 
            Date = DateTime.Now.AddHours(-2), 
            IsRead = true,
            IsDelete = false,
            ActionUrl = "https://www.google.com/search?q=hola"
        }
    };
    
    private int UnreadCount => Notifications.Count(n => !n.IsRead);
    private int DeletedCount => Notifications.Count(n => !n.IsDelete);
    
    private void MarkAsRead(string id)
    {
        NotificationModel? n = Notifications.FirstOrDefault(x => x.Id == id);
        
         if (n != null) n.IsRead = true;
    }
    
    private void MarkAllAsRead() => Notifications.ForEach(n => n.IsRead = true);

    private void RemoveNotification(string id) => Notifications.RemoveAll(x => x.Id == id);
    
    private string GetNotificationClasses(NotificationModel n) =>
        n.IsRead
            ? "p-3 bg-white/80  dark:bg-gray-800"
            : "p-3 rounded-lg bg-gray-400 dark:bg-gray-700";
    
    private RenderFragment GetNotificationIcon(NotificationType type) => builder =>
    {
        switch (type)
        {
            case NotificationType.Success:
                builder.OpenComponent<CheckCircle>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4 text-green-500");
                builder.CloseComponent();
                break;
            case NotificationType.Warning:
                builder.OpenComponent<AlertCircle>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4 text-yellow-500");
                builder.CloseComponent();
                break;
            case NotificationType.Error:
                builder.OpenComponent<Close>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4 text-red-500");
                builder.CloseComponent();
                break;
            default:
                builder.OpenComponent<Info>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4 text-blue-500");
                builder.CloseComponent();
                break;
        }
    };
}