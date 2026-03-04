using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using AppTiemposV3.SharedClases.GenericModels;
using AppTiemposV3.Web.Authentication;
using AppTiemposV3.Web.Components.Icons;
using AppTiemposV3.Web.Pages.Activities.Modales;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Claims;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.Web.Utils.Helpers;

namespace AppTiemposV3.Web.Components.UI;

public partial class Sidebar : ComponentBase, IDisposable
{
    [Inject] private NavigationManager Nav { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] ActivityStateService ActivityState { get; set; } = null!;
    [Inject] private AuthenticationStateProvider AuthStateProvider { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IActivityContract<ActivityResponseDto>  ActivityService { get; set; } = null!;
    [Inject] private IConfigurationContract _configurationService { get; set; } = default!;
    private string CurrentUrl => Nav.Uri.Replace(Nav.BaseUri, "/");
    private string rutaProfile { get; set; } = "/app/perfil";
    private string FechaHoy = string.Empty;
    [Parameter] public bool IsClosed { get; set;  } = false;
    public bool IsProgressLoaded { get; set;  } = false;
    private string TodayHoursWorked { get; set; } = string.Empty;
    private double TodayHoursWorkedDbl { get; set; } = 0;
    private double TodayHoursTarget { get; set; } = 8;
    private int ProgressHour { get; set; } = 0;
    private string HorasFaltantes { get; set; } = string.Empty;

    private bool IsNewButtonDisabled = false;
    private List<ActivityResponseDto> Activities = [];
    #region Modales
    private Guid IdModal = Guid.NewGuid();
    private NewActivitySidebar? newModalRef;
    #endregion
    
    protected override async Task OnInitializedAsync()
    {
        AuthenticationState? authState = await AuthStateProvider.GetAuthenticationStateAsync();
        ClaimsPrincipal? user = authState.User;

        if (user.Identity is { IsAuthenticated: true })
        {
            await GetProgressDay();
        }

        string currentUrl = CurrentUrl;
        ColorService.OnColorChanged += HandleColorChanged;
        if (currentUrl.StartsWith("/app/", StringComparison.OrdinalIgnoreCase))
        {
            Nav.LocationChanged += OnLocationChanged;
            CheckIfDisableNewButton();
            _ = SafeRunAsync(async () => await GetAllActivities());
        }
        State.OnDisabledButtonChanged += HandleDisabledButtonChanged;
        ActivityState.OnActivityUpdated += RefreshData;
    }

    private async void RefreshData()
    {
        await GetProgressDay();
        // 1. Opcional: Volver a poner isLoading = true si traes datos de BD
        // 2. Ejecutar la lógica para recalcular TodayHoursWorked

        // 3. Forzar el refresco de la UI
        await InvokeAsync(StateHasChanged);
    }

    protected override void OnInitialized()
    {
        DateTime hoy = DateTime.Now;
        CultureInfo? cultura = new CultureInfo("es-ES");
        FechaHoy = hoy.ToString("dddd dd/MM", cultura);
        FechaHoy = char.ToUpper(FechaHoy[0]) + FechaHoy.Substring(1);
    }
    
    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        CheckIfDisableNewButton();
        await InvokeAsync(StateHasChanged);
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }

    private List<MenuItem> MenuItems = new()
    {
        new MenuItem("Dashboard", "/app/dashboard", GetItemIcon("Dashboard")),
        new MenuItem("Vista Semanal", "/app/semanal", GetItemIcon("Semanal")),
        new MenuItem("Actividades", "/app/actividades", GetItemIcon("Actividades")),
        new MenuItem("Requerimientos", "/app/requerimientos", GetItemIcon("Requerimientos")),
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
        string currentUrl = CurrentUrl;

        bool isActive = CurrentUrl == url;

        if (!isActive)
        {
            if (url == "/app/actividades" && currentUrl.StartsWith("/app/actividad/", StringComparison.OrdinalIgnoreCase))
                isActive = true;
            
            if (url == "/app/rechazos" && currentUrl.StartsWith("/app/rechazo/", StringComparison.OrdinalIgnoreCase))
                isActive = true;

            if (url == "/app/reportes" && currentUrl.StartsWith("/app/reportes/", StringComparison.OrdinalIgnoreCase))
                isActive = true;

            if (url == "/app/reportes" && currentUrl.StartsWith("/app/reporte/", StringComparison.OrdinalIgnoreCase))
                isActive = true;
        }

        ColorModel? currentColor = ColorService.GetCurrentColor();
        return $"flex items-center gap-3 px-3 py-2 rounded-lg text-sm my-2 " +
               (isActive
                   ? $"{currentColor.Gradient} text-gray-100"
                   : $"{currentColor.Hover} text-gray-700 dark:text-gray-300 hover:text-gray-300");
    }
    
    private string GetPerfilClass(string url, bool isClosed)
    {
        string currentUrl = CurrentUrl;

        bool isActive = CurrentUrl == url;
        
        if (!isActive)
        {
            if (url == "/app/perfil" && currentUrl.StartsWith("/app/perfil/", StringComparison.OrdinalIgnoreCase))
                isActive = true;
        }

        ColorModel? currentColor = ColorService.GetCurrentColor();
        return $"flex items-center gap-3 mb-3 w-full rounded-lg hover:bg-gray-100  cursor-pointer { currentColor!.Hover} " +
               (isClosed ? " ": " p-2 ") +
               (isActive
                   ? $" {currentColor.Gradient} text-gray-100"
                   : $" {currentColor.Hover} text-gray-700 dark:text-gray-300 hover:text-gray-300");
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
        if (TodayHoursWorkedDbl >= TodayHoursTarget)
            return "font-medium text-green-600 dark:text-green-400";

        return "font-medium text-blue-600 dark:text-blue-400";
    }
    
    private async Task NewActivity()
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
        await newModalRef!.ShowAsync(IdModal, hoy);
    }
    
    #region Funciones
    private void NotifyActivityChanged(SavedEventArgs args)
    {
        State.NotifyStateChangedAct(args);
    }
    private async Task HandleActivitySaved(SavedEventArgs args)
    {
        try
        {
            Console.WriteLine("[Parent] 1️⃣ Inicia HandleActivitySaved");
            NotifyActivityChanged(args);
            StateHasChanged();
            Console.WriteLine("[Parent] 2️⃣ StartDate recibido -> " + args.StartDate);

            if (args.Success)
                Toltip.Success("Éxito!", args.Message);
            else
                Toltip.Error("Error", args.Message);

            Console.WriteLine("[Parent]  Llamando a CheckIfDisableNewButton");
            CheckIfDisableNewButton();
            await GetAllActivities();
            Console.WriteLine("[Parent]  Terminó CheckIfDisableNewButton");


            StateHasChanged();
            Console.WriteLine("[Parent] Fin del método completo");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Parent] ❌ Error general: {ex}");
        }
    }
    
    private void HandleDisabledButtonChanged()
    {
        // Refresca los datos o actualiza el estado del botón
        InvokeAsync(async () =>
        {
            await GetAllActivities();
            CheckIfDisableNewButton();
            StateHasChanged();
        });
    }
    
    private void CheckIfDisableNewButton()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        DateOnly selectedDate = DateOnly.Parse(DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        IsNewButtonDisabled = selectedDate < today ||  Activities.Any(a => a.EndTime is null);
    }
    
    private async Task GetAllActivities()
    {
        StateHasChanged();
        
        try
        {
            // TODO: Hay que hacer un enpoint sin paginado y que diga si endtime hay alguno null para la fecha buscada
           
            DataAResponse<ActivityResponseDto> activities =
                await ActivityService.GetAllActivitiesPerDay(DateOnly.Parse(DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
            
            Activities = activities.Data;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            CheckIfDisableNewButton();
            StateHasChanged();
        }
    }


    private async Task GetProgressDay()
    {
        IsProgressLoaded = false;
        StateHasChanged();
        try
        {
            DataResponse<ProgressHoursConfigDto> response = await _configurationService.ProgressHours();

            if (response.Success)
            {
                ProgressHoursConfigDto data = response.Data;
                TodayHoursWorked = data.HorasRealizadas;
                TodayHoursWorkedDbl = data.HorasRealizadasDbl;
                TodayHoursTarget = data.MetaDelDia;
                HorasFaltantes = data.HorasFaltantes;
                ProgressHour = data.Porcentaje;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"{ex.Message}");
            throw;
        }
        finally
        {
            IsProgressLoaded = true;
            StateHasChanged();
        }
    }
    #endregion
    
    public void Dispose()
    {
        Nav.LocationChanged -= OnLocationChanged;
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnDisabledButtonChanged -= HandleDisabledButtonChanged;
        ActivityState.OnActivityUpdated -= RefreshData;
    }
    
    #region Scripts
    private async Task SafeRunAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error en fire & forget: {ex}");
        }
    }
    #endregion
   
}