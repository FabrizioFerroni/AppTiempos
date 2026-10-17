using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.Enums;
using AppTiemposV3.Web.Components.Icons;
using AppTiemposV3.Web.Pages.Activities.Modales;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Globalization;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;


namespace AppTiemposV3.Web.Pages.Activities;

public partial class Details : ComponentBase, IDisposable
{
    [Parameter] public string urlId { get; set; } = string.Empty;
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private IActivityContract<ActivityResponseDto>  ActivityService { get; set; } = null!;
    [Inject] private NavigationManager Nav { get; set; } = null!;

    private bool _mostrarDetalleEtapas;

    private void ToggleDetalleEtapas() => _mostrarDetalleEtapas = !_mostrarDetalleEtapas;

    private bool IsLoadingData { get; set; } = true;
    private ActivityResponseDto? ActivityRes { get; set; }
    
    private record StatusItem(RenderFragment Icon, string Result);
    #region Modales
    private Guid IdModalEdit = Guid.NewGuid();
    private ShowRequeriment? showModalRef;
    private EditActivity? editModalRef;
    private DeleteActivity? deleteModalRef;
    #endregion
    
    #region Inicializacion

    protected async override Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();
        await GetActivity();
    }
    #endregion

    #region Funciones
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    private void HandleSidebarToggle()
    {
        _ = State.ToggleSidebar();
    }
    
    private async Task HandleActivitySaved(SavedEventArgs args)
    {
        if (args.Success)
        {
            Toltip.Success("Éxito!", args.Message);
        }
        else
        {
            Toltip.Error("Error", args.Message);
        }
        
        await GetActivity();
    }
    
    private Task HandleActivityDeleted(DateOnly startDate)
    {
        Nav.NavigateTo("/app/actividades");
        return Task.CompletedTask;
    }

    private async Task GetActivity()
    {
        IsLoadingData = true;
        StateHasChanged();
        try
        {
            DataResponse<ActivityResponseDto> activity = await ActivityService.GetActivityByUrl(urlId);

            if (!activity.Success)
            {
                Nav.NavigateTo("/app/actividades");
            }
            
            ActivityRes = activity.Data;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            IsLoadingData = false;
            StateHasChanged();
        }
    }

    private string ObtainDate(DateOnly date)
    {
        CultureInfo? cultura = new CultureInfo("es-ES");
        string fecha = date.ToString("dddd", cultura);
        fecha = char.ToUpper(fecha[0]) + fecha.Substring(1);
        return fecha;
    }
    
    private string ConvertHour(TimeSpan hour)
    {
        CultureInfo? cultura = new CultureInfo("es-ES");
        string fecha = hour.ToString("HH:mm", cultura);
        return fecha;
    }

    private string ObtenerTiempoDesarrolloSP(string storypoints)
    {        
        if (!double.TryParse(storypoints, NumberStyles.Any, CultureInfo.InvariantCulture, out double storyPoints))
            storyPoints = 0.0;
        
        if (storyPoints == 0.0)
        {
            return "0h 00m";
        }

        double porcentaje = 1;
        int totalSeconds = (int)Math.Round(storyPoints * porcentaje * 3600.0);

        int horas = totalSeconds / 3600;
        int minutos = (totalSeconds % 3600) / 60;
        string result = "";
        
        if (horas > 0)
            return $"{horas}h {minutos:d2}m";
        if (minutos > 0)
            return $"{minutos}m";

        return result;
    }
    
    private string ObtainTimeEstimated(string storyPointsStr, string area)
    {
        if (!double.TryParse(storyPointsStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double storyPoints))
            storyPoints = 0.0;

        if (storyPoints == 0.0)
        {
            return "0h 00m";
        }

        double porcentaje = 0;
        switch (area)
        {
            case "Desarrollo":
                porcentaje = 0.8;
                break;
            case "Testing":
                porcentaje = 0.2;
                break;
            default:
                porcentaje = 1;
                break;
        }
        
        int totalSeconds = (int)Math.Round(storyPoints * porcentaje * 3600.0);
        
        int horas = totalSeconds / 3600;
        int minutos = (totalSeconds % 3600) / 60;
        string result = "";
        
        if (horas > 0)
            return $"{horas}h {minutos:d2}m";
        if (minutos > 0)
            return $"{minutos}m";

        return result;
        
    }

    private string FormatTime(TimeSpan ts)
    {
        if (ts == TimeSpan.Zero)
        {
            return "0h 00m";
        }

        int horas = (int)ts.TotalHours;
        int minutos = ts.Minutes;

        return $"{horas}h {minutos:00}m";
    }

    private TimeSpan ObtainTimeEstimatedNew(string storyPointsStr, string area)
    {
        if (!double.TryParse(storyPointsStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double storyPoints))
            storyPoints = 0;

        double porcentaje = area switch
        {
            "Desarrollo" => 0.8,
            "Testing" => 0.2,
            _ => 1
        };

        int totalSeconds = (int)Math.Round(storyPoints * porcentaje * 3600.0);

        return TimeSpan.FromMinutes(totalSeconds / 60);
    }

    private string ObtainTimeEstimatedHMS(string storyPointsStr, string area)
    {
        TimeSpan ts = ObtainTimeEstimatedNew(storyPointsStr, area);
        return $"{(int)ts.TotalHours:00}:{ts.Minutes:00}:00";
    }

    private static bool TieneDetalleEtapas(ActivityResponseDto act)
    {
        TimeSpan[] tiempos =
        {
            act.WorkedTimeAlta,
            act.WorkedTimeAprobacion,
            act.WorkedTimeEvaluacion,
            act.WorkedTimeEjecucion,
            act.WorkedTimePrueba,
            act.WorkedTimeImplementacion,
            act.WorkedTimeValidacion
        };

        return tiempos.Count(t => t > TimeSpan.Zero) >= 2;
    }

    private static TimeSpan GetWorkedTimeActual(ActivityResponseDto act) => act.Requeriment.EtapaActual switch
    {
        Etapas.Alta => act.WorkedTimeAlta,
        Etapas.Aprobacion => act.WorkedTimeAprobacion,
        Etapas.Evaluacion => act.WorkedTimeEvaluacion,
        Etapas.Ejecucion => act.WorkedTimeEjecucion,
        Etapas.Prueba => act.WorkedTimePrueba,
        Etapas.Implementacion => act.WorkedTimeImplementacion,
        Etapas.Validacion => act.WorkedTimeValidacion,
        _ => TimeSpan.Zero
    };

    private StatusItem ObtainStatus(TimeSpan workedTime, string storypoints, string area, Etapas? etapa, string? category)
    {
        TimeSpan estimatedTime = ObtainTimeEstimatedNew(storypoints, area);

        TimeSpan worked = TimeSpan.FromMinutes(Math.Floor(workedTime.TotalMinutes));

        // Solo se evalúa en ejecución, para Desarrollo o Testing, y si existe una estimación
        bool aplicaControl = etapa == Etapas.Ejecucion
                             && (area == "Desarrollo" || area == "Testing")
                             && estimatedTime > TimeSpan.Zero;

        string status = string.Empty;

        if (aplicaControl)
        {
            if (worked > estimatedTime) status = "over";
            else if (worked < estimatedTime) status = "under";
            else status = "within";
        }

        return new StatusItem(GetItemIcon(status), GetTimeStatusText(status));
    }

    private static string FormatTimeNew(TimeSpan time) => $"{(int)time.TotalHours}h {time.Minutes:00}m";

    private List<(string Nombre, TimeSpan Tiempo, bool EsActual)> GetTiemposPorEtapa(ActivityResponseDto activity)
    {
        Etapas actual = (Etapas)activity.Requeriment.EtapaActual!;

        (Etapas Etapa, TimeSpan Tiempo)[] etapas =
        {
            (Etapas.Alta, activity.WorkedTimeAlta),
            (Etapas.Aprobacion, activity.WorkedTimeAprobacion),
            (Etapas.Evaluacion, activity.WorkedTimeEvaluacion),
            (Etapas.Ejecucion, activity.WorkedTimeEjecucion),
            (Etapas.Prueba, activity.WorkedTimePrueba),
            (Etapas.Implementacion, activity.WorkedTimeImplementacion),
            (Etapas.Validacion, activity.WorkedTimeValidacion)
        };

        return etapas
            .Where(e => e.Tiempo > TimeSpan.Zero || e.Etapa == actual)
            .OrderByDescending(e => e.Etapa)
            .Select(e => (e.Etapa.ToString(), e.Tiempo, e.Etapa == actual))
            .ToList();
    }    

    private static RenderFragment GetItemIcon(string status) => builder =>
    {
        switch (status)
        {
            case "under":
                builder.OpenComponent<CheckCircle>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4 text-green-600");
                builder.CloseComponent();
                break;
            
            case "within":
                builder.OpenComponent<AlertTriangle>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4");
                builder.AddAttribute(2, "Style", "color: #F0B100 !important;");
                builder.CloseComponent();
                break;
           
            case "over":
                builder.OpenComponent<XCircle>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4 text-red-600");
                builder.CloseComponent();
                break;
            
            default:
                builder.OpenComponent<Clock>(0);
                builder.AddAttribute(1, "Class", "h-4 w-4 text-gray-600");
                builder.CloseComponent();
                break;
        }
    };
    
    private string GetTimeStatusText(string status) {
        switch (status) {
            case "under":
                return "Por debajo del tiempo asignado";
            case "over":
                return "Fuera del tiempo asignado";
            case "within":
                return "Dentro del tiempo asignado";
            default:
                return "Sin definir";
        }
    }
    #endregion
    
    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
    }
    #endregion
    
    #region ModalesFunciones
    
    private async Task ShowModal(Guid id)
    {
        await showModalRef!.ShowAsync(id);
    }
    
    private async Task EditModal(Guid idActivity)
    {
        await editModalRef!.ShowAsync(IdModalEdit, idActivity);
    }
    
    private async Task DeleteModal(Guid id, string reqId, DateOnly startDate)
    {
        await deleteModalRef!.ShowAsync(id, reqId, startDate);
    }
    #endregion
}