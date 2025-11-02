using System.Globalization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.Web.Components.Icons;
using AppTiemposV3.Web.Pages.Activities.Modales;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
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
        /*double storyPointsStr = Double.Parse(storypoints, CultureInfo.InvariantCulture);
        if (storypoints == "0:00")
        {
            return "0h 00m";
        }
        
        int horas = (int)storyPointsStr;
        int minutos = (int)Math.Round((storyPointsStr - horas) * 60);
        
        return $"{horas.ToString("0")}m {minutos.ToString("00")}m";*/
        
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

        /*double horasEstimadas = storyPoints * porcentaje;
        int horas = (int)horasEstimadas;
        int minutos = (int)Math.Round((horasEstimadas - horas) * 60);

        return $"{horas.ToString("0")}m {minutos.ToString("00")}m";*/
        
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
    
    private string ObtainTimeEstimatedHMS(string storyPointsStr, string area)
    {
        // Parsear de forma segura con InvariantCulture
        if (!double.TryParse(storyPointsStr, NumberStyles.Any, CultureInfo.InvariantCulture, out double storyPoints))
            storyPoints = 0;
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
        
        double horasEstimadas = storyPoints * porcentaje;

        // Convertir a segundos totales y redondear
        int totalSeconds = (int)Math.Round(horasEstimadas * 3600.0);

        int horas = totalSeconds / 3600;
        int minutos = (totalSeconds % 3600) / 60;
        int segundos = 0;

        return $"{horas:00}:{minutos:00}:{segundos:00}";
    }

    private StatusItem ObtainStatus(TimeSpan workedTime, string storypoints, string area)
    {
        string storyPointInHour = ObtainTimeEstimatedHMS(storypoints, area);
        TimeSpan timeSpan = new TimeSpan(workedTime.Hours, workedTime.Minutes, 0);
        
        Console.WriteLine($"Tiempo total del requerimiento: {storyPointInHour}");
        Console.WriteLine($"Tiempo total de las tareas: {timeSpan}");

        // Parseamos ese string a TimeSpan
        
        TimeSpan estimatedTime;
        if (!TimeSpan.TryParse(storyPointInHour, out estimatedTime))
        {
            // Si viene algo como "50:27:00", tratamos los dos primeros como minutos
            string[] parts = storyPointInHour.Split(':');
            if (parts.Length == 3 && int.Parse(parts[0]) >= 24) // más de 24 horas => probablemente minutos
            {
                int minutes = int.Parse(parts[0]);
                int seconds = int.Parse(parts[1]);
                int milliseconds = int.Parse(parts[2]);
                estimatedTime = new TimeSpan(0, minutes, seconds); // hh:mm:ss -> 0h + X min + Y sec
            }
            else
            {
                estimatedTime = TimeSpan.Zero;
            }
        }

        // Determinamos el estado comparando los tiempos
        string status;
        
        if (timeSpan > estimatedTime)
            status = "over";
        else if (timeSpan < estimatedTime)
            status = "under";
        else
            status = "within";

        // Creamos el texto descriptivo
        string resultTxt = GetTimeStatusText(status);

        // Devolvemos el objeto con su icono y texto
        StatusItem st = new StatusItem(GetItemIcon(status), resultTxt);
        return st;
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