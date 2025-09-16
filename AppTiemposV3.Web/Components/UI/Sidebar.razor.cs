using AppTiemposV3.Web.Authentication;
using AppTiemposV3.Web.Components.Icons;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Globalization;
using AppTiemposV3.SharedClases.GenericModels;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;

namespace AppTiemposV3.Web.Components.UI;

public partial class Sidebar : ComponentBase
{
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
    private string CurrentUrl => Nav.Uri.Replace(Nav.BaseUri, "/");
    private string rutaAcciones { get; set; } = "/app/actividades?nueva=true";
    private string rutaProfile { get; set; } = "/app/perfil";
    private string FechaHoy = string.Empty;
    [Parameter] public bool IsClosed { get; set;  } = false;
    private double TodayHoursWorked { get; set; } = Math.Round(4.50, 1);
    private double TodayHoursTarget { get; set; } = 8;
    
    protected override async Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
    }
    
    protected override void OnInitialized()
    {
        DateTime hoy = DateTime.Now;
        var cultura = new CultureInfo("es-ES");

        FechaHoy = hoy.ToString("dddd dd/MM", cultura);

        FechaHoy = char.ToUpper(FechaHoy[0]) + FechaHoy.Substring(1);
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    
    private string GetRemainingHours()
    {
        double remaining = TodayHoursTarget - TodayHoursWorked;

        if (remaining <= 0)
            return "0h";

        // Convierte a horas y minutos reales
        int hours = (int)remaining;
        int minutes = (int)Math.Round((remaining - hours) * 60);

        // Convertimos minutos a fracción decimal (ej: 30 min = 0.5h, 6 min = 0.1h)
        double totalDecimal = hours + (minutes / 60.0);

        return totalDecimal.ToString("0.0") + "h";
    }

    private List<MenuItem> MenuItems = new()
    {
        new MenuItem("Dashboard", "/app/dashboard", GetItemIcon("Dashboard")),
        new MenuItem("Vista Semanal", "/app/semanal", GetItemIcon("Semanal")),
        new MenuItem("Actividades", "/app/actividades", GetItemIcon("Actividades")),
        new MenuItem("Requerimientos", "/app/requiremientos", GetItemIcon("Requerimientos")),
        new MenuItem("Capacitaciones", "/app/capacitaciones", GetItemIcon("Capacitaciones")),
        new MenuItem("Rechazos", "/app/rechazos", GetItemIcon("Rechazos")),
        new MenuItem("Reportes", "/app/reportes", GetItemIcon("Reportes")),
        new MenuItem("Configuración", "/app/configuraciones", GetItemIcon("Configuración")),
    };

    private List<MenuItem> AdminMenuItems = new()
    {
        new MenuItem("Auditorias", "/app/auditorias", GetItemIcon("Auditorias")),
        new MenuItem("Invitaciones", "/app/invitaciones", GetItemIcon("Invitaciones")),
    };
    
    private async Task Logout()
    {
        CustomAuthenticationProvider? customAuthStateProvider = (CustomAuthenticationProvider)AuthStateProvider;
        await customAuthStateProvider.UpdateAuthenticationState(null!);
        Nav.NavigateTo("/");
    }

    private record MenuItem(string Title, string Url, RenderFragment Icon);
    
    private string GetInitials(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return string.Empty;

        string[] parts = fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return string.Empty;

        if (parts.Length == 1)
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

        return $"{parts[0][0]}{parts[1][0]}".ToUpper();
    }

    private string GetItemClass(string url)
    {
        bool isActive = CurrentUrl == url;
        ColorModel? currentColor = ColorService.GetCurrentColor();
        return $"flex items-center gap-3 px-3 py-2 rounded-lg text-sm my-2 " +
               (isActive
                   ? $"{currentColor.Gradient} text-gray-100"
                   : $"{currentColor.Hover} text-gray-700 dark:text-gray-300 hover:text-gray-300");
    }
    
    private static RenderFragment GetItemIcon(string menu) => builder =>
    {
        switch (menu)
        {
            case "Dashboard":
                builder.OpenComponent<HomeIcon>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Semanal":
                builder.OpenComponent<CalendarClock>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Actividades":
                builder.OpenComponent<Clock>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Requerimientos":
                builder.OpenComponent<FileText>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Capacitaciones":
                builder.OpenComponent<GraduationCap>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Rechazos":
                builder.OpenComponent<XCircle>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Reportes":
                builder.OpenComponent<BarChart3>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Configuración":
                builder.OpenComponent<Settings>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Invitaciones":
                builder.OpenComponent<UserPlus>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            case "Auditorias":
                builder.OpenComponent<BookLock>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.CloseComponent();
                break;
            default:
                break;
        }
    };
    
    private string GetPercentageClass()
    {
        if (TodayHoursWorked >= TodayHoursTarget)
            return "font-medium text-green-600 dark:text-green-400";

        return "font-medium text-blue-600 dark:text-blue-400";
    }
    
    
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
    }
   
}