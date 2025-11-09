using System.Globalization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.Web.Components.UI;
using AppTiemposV3.Web.Services;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using static System.Globalization.CultureInfo;
using static AppTiemposV3.Web.Utils.CssHelper;
using static AppTiemposV3.Web.Utils.ConsolePrintHelper;
namespace AppTiemposV3.Web.Pages;

public partial class Home: ComponentBase, IDisposable
{
    #region Variables
    #region InjeccionesDep
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IActivityContract<ActivityResponseDto>  ActivityService { get; set; } = null!;
    [Inject] private IDashboardContract<DashboardKPIDto> DashboardService { get; set; } = null!;
    #endregion
    
    private DateTime currentDate = DateTime.Today;
    private string weekRange = string.Empty;
    private bool isNextDisabled = false;
    private bool isPrevDisabled = true;
    private bool isWeekActual = false;
    private int weekNumberSelected = 1;
    private bool IsNewButtonDisabled = false;
    private List<ActivityResponseDto> Activities = [];
    private List<ActivityResponseDto> ActivitiesThree = [];
    private bool isLoadingAct3 = true;
    private bool isLoadingKPIs = false;
    private DashboardKPIDto kpiData = new();
    private string CurrentUrl => Router.Uri.Replace(Router.BaseUri, "/");
    private void GoToWeekly() => NavigateTo("/app/semanal");
    private void GoToReports() => NavigateTo("/app/reportes");
    
    #region Modales
    private Guid IdModal = Guid.NewGuid();
    private NewActivitySidebar? newModalRef;
    #endregion
    #endregion

    #region Inicializacion

    protected override async Task OnInitializedAsync()
    {
        weekNumberSelected = GetWeekNumber();
        int diff = (7 + (currentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime monday = currentDate.AddDays(-diff);
        DateTime saturday = monday.AddDays(5);

        // Mostrar rango
        weekRange = $"{monday:dd/MM} - {saturday:dd/MM/yyyy}";
        
        DateTime todayMonday = DateTime.Today.AddDays(-(7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7);
        isNextDisabled = monday >= todayMonday;
        isWeekActual = monday == todayMonday;
        string currentUrl = CurrentUrl;
        if (currentUrl.StartsWith("/app/", StringComparison.OrdinalIgnoreCase))
        {
            Router!.LocationChanged += OnLocationChanged;
            CheckIfDisableNewButton();
            _ = SafeRunAsync(async () => await GetAllActivities());
        }
        ColorService.OnColorChanged += HandleColorChanged;
        State.OnSidebarChanged += StateHasChanged;

        await GetKpis();
        await GetLastThreeActivities();
        await State.InitializeAsync();
        
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

    #endregion
    private void HandleSidebarToggle()
    {
        _ = State.ToggleSidebar();
    }

    #region Funciones

    private async Task GetKpis(bool loadKpis = true)
    {
        isLoadingKPIs = loadKpis;
        StateHasChanged();
        try
        {
            int year = currentDate.Year;
            int weekNumber = GetWeekNumber();
            DataResponse<DashboardKPIDto> data = await DashboardService.GetKpiDashboard(year, weekNumber);
            if (data.Success)
            {
                kpiData = data.Data;
                StateHasChanged();
            }
            else
            {
                kpiData = new()
                {
                    TotalHours = 0,
                    CompletedTasks = 0,
                    PendingTasks = 0,
                    DashboardKPIChart = new List<DashboardKPIChart>()
                    {
                        new DashboardKPIChart()
                        {
                            Day = "Lunes",
                            DayNumber = 1,
                            HoursTotal = 0
                        },
                        new DashboardKPIChart()
                        {
                            Day = "Martes",
                            DayNumber = 2,
                            HoursTotal = 0
                        },
                        new DashboardKPIChart()
                        {
                            Day = "Miercoles",
                            DayNumber = 3,
                            HoursTotal = 0
                        },
                        new DashboardKPIChart()
                        {
                            Day = "Jueves",
                            DayNumber = 4,
                            HoursTotal = 0
                        },
                        new DashboardKPIChart()
                        {
                            Day = "Viernes",
                            DayNumber = 5,
                            HoursTotal = 0
                        },
                        new DashboardKPIChart()
                        {
                            Day = "Sabado",
                            DayNumber = 6,
                            HoursTotal = 0
                        }
                    }
                };
            }
        }
        catch (Exception e)
        {
            kpiData = new()
            {
                TotalHours = 0,
                CompletedTasks = 0,
                PendingTasks = 0,
                DashboardKPIChart = new List<DashboardKPIChart>()
                {
                    new DashboardKPIChart()
                    {
                        Day = "Lunes",
                        DayNumber = 1,
                        HoursTotal = 0
                    },
                    new DashboardKPIChart()
                    {
                        Day = "Martes",
                        DayNumber = 2,
                        HoursTotal = 0
                    },
                    new DashboardKPIChart()
                    {
                        Day = "Miercoles",
                        DayNumber = 3,
                        HoursTotal = 0
                    },
                    new DashboardKPIChart()
                    {
                        Day = "Jueves",
                        DayNumber = 4,
                        HoursTotal = 0
                    },
                    new DashboardKPIChart()
                    {
                        Day = "Viernes",
                        DayNumber = 5,
                        HoursTotal = 0
                    },
                    new DashboardKPIChart()
                    {
                        Day = "Sabado",
                        DayNumber = 6,
                        HoursTotal = 0
                    }
                }
            };
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            isLoadingKPIs = false;
            StateHasChanged();
        }
    }

    private string GetClassesStatus(string status)
    {
        string baseClasses =
            $"inline-flex items-center px-2 py-1 rounded-full text-xs font-medium";

        string baseStatus = status == "completed" ? " bg-green-100 dark:bg-green-900/30 text-green-800 dark:text-green-200" : "bg-yellow-100 dark:bg-yellow-900/30 text-yellow-800 dark:text-yellow-200";

        return Cn(baseClasses, baseStatus);
    }

    private string GetStatusName(string status)
    {
        switch (status) {
            case "completed":
                return "Completado";
            case "in-progress":
                return "En Progreso";
            case "pending":
                return "Pendiente";
            default:
                return status;
        }
    }
    
    private void NavigateTo(string url)
    {
        Router!.NavigateTo(url, false, false);
    }
    
    private async Task NewActivity()
    {
        DateOnly hoy = DateOnly.FromDateTime(DateTime.Now);
        await newModalRef!.ShowAsync(IdModal, hoy);
    }
    
    private void EnableButtonChanged()
    {
        State.NotifyButtonStatusChanged();
    }
    
    private async Task HandleActivitySaved(SavedEventArgs args)
    {
        try
        {
            NotifyActivityChanged(args);
            StateHasChanged();

            if (args.Success)
            {
                Toltip.Success("Éxito!", args.Message);
                EnableButtonChanged();
            }
            else
            {
                Toltip.Error("Error", args.Message);
            }
            
            await GetAllActivities();

            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Parent] ❌ Error general: {ex}");
        }
    }
    
    private void CheckIfDisableNewButton()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Now);
        DateOnly selectedDate = DateOnly.Parse(DateTime.Today.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        IsNewButtonDisabled = selectedDate < today || Activities.Any(a => a.EndTime is null);
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
            StateHasChanged();
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
    
    private async Task GetLastThreeActivities()
    {
        isLoadingAct3 = true;
        StateHasChanged();
        
        try
        {
            int year = currentDate.Year;
            int weekNumber = GetWeekNumber();
            DataAResponse<ActivityResponseDto> activities = await ActivityService.GetLastThreeActivities(year, weekNumber);
            
            ActivitiesThree = activities.Data;
            StateHasChanged();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            isLoadingAct3 = false;
            StateHasChanged();
        }
    }
    
    private void NotifyActivityChanged(SavedEventArgs args)
    {
        State.NotifyStateChangedAct(args);
    }
    
    void UpdateWeekRange()
    {
        int diff = (7 + (currentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime monday = currentDate.AddDays(-diff);
        DateTime saturday = monday.AddDays(5);

        // Mostrar rango
        weekRange = $"{monday:dd/MM} - {saturday:dd/MM/yyyy}";
        

        // Deshabilitar avanzar si ya estamos en la semana actual o futura
        DateTime todayMonday = DateTime.Today.AddDays(-(7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7);
        isNextDisabled = monday >= todayMonday;
        isWeekActual = monday == todayMonday;
        
        _ = SafeRunAsync(async () => await GetKpis());
        _ = SafeRunAsync(async () => await GetLastThreeActivities());
    }
    
    void NextWeek()
    {
        if (!isNextDisabled)
        {
            weekNumberSelected = GetWeekNumber();
            currentDate = currentDate.AddDays(7);
            UpdateWeekRange();
        }
    }

    void PreviousWeek()
    {
        weekNumberSelected = GetWeekNumber();
        currentDate = currentDate.AddDays(-7);
        UpdateWeekRange();
    }
    
    int GetWeekNumber()
    {
        CultureInfo ci = CurrentCulture;

        return ci.Calendar.GetWeekOfYear(
            currentDate,
            CalendarWeekRule.FirstFourDayWeek,  
            DayOfWeek.Monday                    
        );
    }
    
    private string ObtenerDiaSemana(string fechaStr)
    {
        if (!DateTime.TryParse(fechaStr, out DateTime fecha))
            return string.Empty; 

        string dia = fecha.ToString("dddd", new CultureInfo("es-ES"));
        return char.ToUpper(dia[0]) + dia.Substring(1);
    }
    #endregion
    
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
    
    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
    }
    #endregion
}