using System.Globalization;
using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.Web.Components.UI;
using AppTiemposV3.Web.Services;
using ChartJs.Blazor.Common;
using ChartJs.Blazor.LineChart;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Microsoft.VisualBasic.CompilerServices;
using static System.Globalization.CultureInfo;
using static AppTiemposV3.Web.Utils.CssHelper;
using static AppTiemposV3.Web.Utils.ConsolePrintHelper;
using System.Text.Json;
using ChartJs.Blazor.BarChart;
using ChartJs.Blazor.Util;


namespace AppTiemposV3.Web.Pages;

public partial class Home: ComponentBase, IDisposable
{
    #region Variables
    #region InjeccionesDep
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; } = null!;
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
    
    #region Graficos
    private BarConfig configGraficos = new();
    private bool chartReady = false;
    #endregion
    
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
    private async Task FixChartScaleAsync()
    {
        if (!chartReady)
        {
            return;
        }

        try
        {
            await JS.InvokeVoidAsync("fixChartScale", configGraficos.CanvasId);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al ajustar escala: {ex.Message}");
        }
    }
    
    private async Task OnChartReady()
    {
        chartReady = true;
        await FixChartScaleAsync();
    }
    
    private async Task LoadGraficosHours()
    {
        configGraficos = new BarConfig
        {
            Options = new BarOptions
            {
                Responsive = true,
                Legend = new Legend()
                {
                  Display = false,  
                },
                Plugins = new Plugins
                {
                    Legend = new Legend
                    {
                        Display = false 
                    },
                    Title = new Title
                    {
                        Display = true,
                        Text = "Horas por Día"
                    }
                },
            }
        };

        BarDataset<double> ds = new BarDataset<double>
        {
            Label = "Horas trabajadas",
            BorderWidth = 1
        };

        List<string> colors = new();

        foreach (DashboardKPIChart dkpi in kpiData.DashboardKPIChart)
        {
            configGraficos.Data.Labels.Add(dkpi.Day);
            ds.Add(Math.Max(0, dkpi.HoursTotal));
            colors.Add(ColorUtil.RandomColorString());
        }
        
        if (ds.Data.Count == 0)
        {
            configGraficos.Data.Labels.Add("Sin datos");
            ds.Add(0);
            colors.Add("rgba(200,200,200,0.3)");
        }

        ds.BackgroundColor = colors.ToArray();

        configGraficos.Data.Datasets.Add(ds);
        
        double maxValue = ds.Data.Any() ? ds.Data.Max() : 0;
        bool allZeros = ds.Data.All(x => x == 0);
        
        configGraficos.Options.Scales = new Dictionary<string, Axis>
        {
            {
                "y", new Axis
                {
                    Type = "linear",
                    BeginAtZero = true,
                    Min = 0,
                    Max = allZeros ? 1 : maxValue * 1.2,
                    Ticks = new Ticks
                    {
                        MaxTicksLimit = 5
                    },
                    Title = new Title
                    {
                        Display = true,
                        Text = "Horas trabajadas"
                    },
                }
            },
            {
                "x", new Axis
                {
                    Type = "category",
                    Title = new Title
                    {
                        Display = true,
                        Text = "Día"
                    },
                    Ticks = new Ticks
                    {
                        MaxRotation = 0, 
                        MinRotation = 0
                    },
                }
            }
        };
        await FixChartScaleAsync();
        StateHasChanged();
    }
    
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
                kpiData = CreateEmptyKpiData();
            }
        }
        catch (Exception e)
        {
            kpiData = CreateEmptyKpiData();
            Console.WriteLine(e);
        }
        finally
        {
            await LoadGraficosHours();
            isLoadingKPIs = false;
            StateHasChanged();
        }
    }

    private DashboardKPIDto  CreateEmptyKpiData()
    {
        return new DashboardKPIDto
        {
            TotalHours = 0,
            CompletedTasks = 0,
            PendingTasks = 0,
            DashboardKPIChart = new List<DashboardKPIChart>
            {
                new() { Day = "Lunes", DayNumber = 1, HoursTotal = 0 },
                new() { Day = "Martes", DayNumber = 2, HoursTotal = 0 },
                new() { Day = "Miércoles", DayNumber = 3, HoursTotal = 0 },
                new() { Day = "Jueves", DayNumber = 4, HoursTotal = 0 },
                new() { Day = "Viernes", DayNumber = 5, HoursTotal = 0 },
                new() { Day = "Sábado", DayNumber = 6, HoursTotal = 0 }
            }
        };
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

        weekRange = $"{monday:dd/MM} - {saturday:dd/MM/yyyy}";
        
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
