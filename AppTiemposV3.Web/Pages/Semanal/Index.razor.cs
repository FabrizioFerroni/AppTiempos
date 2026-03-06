using AppTiemposV3.SharedClases.Contracts;
using AppTiemposV3.SharedClases.DTOs;
using AppTiemposV3.SharedClases.DTOs.Activities;
using AppTiemposV3.SharedClases.DTOs.Configurations;
using AppTiemposV3.Web.Pages.Requeriments.Modals;
using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using static AppTiemposV3.SharedClases.DTOs.ServiceResponse;
using static AppTiemposV3.SharedClases.Utilidades.DateHelper;
using static AppTiemposV3.Web.Utils.Helpers;
using static System.Globalization.CultureInfo;
using static System.Text.Json.JsonSerializer;
using static System.Text.Json.Serialization.JsonNumberHandling;

namespace AppTiemposV3.Web.Pages.Semanal;

public partial class Index : ComponentBase, IDisposable
{
    #region  Variables
    #region InyeccionDependencias
    [Inject] LayoutState State { get; set; } = null!;
    [Inject] private IJSRuntime? JS { get; set; }
    [Inject] private ILocalStorageService LocalStorageService { get; set; } = default!;
    [Inject] private NavigationManager? Router { get; set; }
    [Inject] private NotificationService Toltip { get; set; } = default!;
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IActivityContract<ActivityResponseDto> ActivitiesService { get; set; } = null!;
    [Inject] private IActivityWeeklyContract<ActivitiesByDay> ActivityWeeklyService { get; set; } = default!;
    #endregion

    private List<ActivitiesByDay> AllActivities = new(); // todos los datos
    private List<ActivitiesByDay> Activities = new(); 
    private string? BuscarPor = "";
    
    private bool IsLoading = true;
   
    private int activeFiltersCount = 0;
    
    private DateTime currentDate = DateTime.Today;
    private string weekRange = string.Empty;
    private  bool isNextDisabled = false;
    private bool IsSelectClosed = true;
    private int weekNumberSelected = 45;
    
    private List<DateTime> WeekDays = new();
    
    private string? EstadoSeleccionado = "";
    private List<string> OptionsEstados = new() {"Pendiente", "En Progreso", "Completado"};

    private FilterAdvanceDto filterDto = new();
    
    #region Modales
    private ShowRequeriment? showModalRef;
    #endregion

    private bool isOpenFiltros = false;
    private bool isLoadingFiltros = false;
    
    private string? search { get; set; } = string.Empty;
    private bool SearchEmpty = false;
    private Dictionary<DiasSemana, double> _configHoras = new();
    #endregion

    #region Inicializacion
    protected override async Task OnInitializedAsync()
    {
        SearchEmpty = false;
        weekNumberSelected = GetWeekNumber();
        int diff = (7 + (currentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime monday = currentDate.AddDays(-diff);
        DateTime saturday = monday.AddDays(5);

        // Mostrar rango
        weekRange = $"{monday:dd/MM} - {saturday:dd/MM/yyyy}";
        
        DateTime todayMonday = DateTime.Today.AddDays(-(7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7);
        isNextDisabled = monday >= todayMonday;
        
        WeekDays = Enumerable.Range(0, 6)
            .Select(offset => monday.AddDays(offset))
            .ToList();

        ColorService.OnColorChanged += HandleColorChanged;
        await GetAllActivities(true);
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();


        try
        {
            string? json = await LocalStorageService.GetItemAsStringAsync("HorasSemanalesConfig");

            if (!string.IsNullOrEmpty(json))
            {
                JsonSerializerOptions? options = new JsonSerializerOptions { 
                    PropertyNameCaseInsensitive = true,
                    NumberHandling = AllowReadingFromString
                };

                options.Converters.Add(new JsonStringEnumConverter());

                List<DayConfig>? lista = Deserialize<List<DayConfig>>(json, options);

                if (lista != null)
                {
                    _configHoras = lista.ToDictionary(x => x.Day, x => x.MinHours);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error cargando config: {ex.Message}");
        }
    }
    #endregion
    
    #region Funciones
    private void DateChangedFrom(DateTime?  date)
    {

        if (filterDto.timeFrom is not null || filterDto.timeTo is not null)
        {
            filterDto.timeFrom = null;
            filterDto.timeTo = null;
            activeFiltersCount--;
        }
        
        
        if (date.HasValue)
        {
            filterDto.dateFrom = date;
        }
        else
        {
            filterDto.dateFrom = new DateTime();
        }
    }
    
    private void DateChangedTo(DateTime?  date)
    {
        if (date.HasValue)
        {
            filterDto.dateTo = date;
        }
        else
        {
            filterDto.dateTo = new DateTime();
        }
        
    }
    
    private Task OnEstadoSelectedChanged(string estado)
    {
        EstadoSeleccionado = OnEstadoSelectedChangedBD(estado);
        filterDto.estadoSel = GetStatusName(estado);
        return Task.CompletedTask;
        
    }

    private string OnEstadoFilter(string estado)
    {
        EstadoSeleccionado = OnEstadoSelectedChangedBD(estado);
        filterDto.estadoSel = GetStatusName(estado);
        return OnEstadoSelectedChangedBD(estado);
    }
    
    private string OnEstadoSelectedChangedBD(string estado)
    {
        EstadoSeleccionado = GetStatusNameR(estado);
        return GetStatusNameR(estado);
        
    }
    
    private string GetStatusName(string status)
    {
        switch (status) {
            case "Completado":
                return "completed";
            case "En Progreso":
                return "in-progress";
            case "Pendiente":
                return "pending";
            default:
                return status;
        }
    }

    private string GetStatusNameR(string status)
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
    
    private void HandleSidebarToggle()
    {
        _ = State.ToggleSidebar();
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    
    private void HandleDropdownState(bool closed)
    {
        IsSelectClosed = closed;
    }
    
    void UpdateWeekRange()
    {
        int diff = (7 + (currentDate.DayOfWeek - DayOfWeek.Monday)) % 7;
        DateTime monday = currentDate.AddDays(-diff);
        DateTime saturday = monday.AddDays(5);

        // Mostrar rango
        weekRange = $"{monday:dd/MM} - {saturday:dd/MM/yyyy}";
        
        // Generar los 7 días
        WeekDays = Enumerable.Range(0, 6)
            .Select(offset => monday.AddDays(offset))
            .ToList();

        // Deshabilitar avanzar si ya estamos en la semana actual o futura
        DateTime todayMonday = DateTime.Today.AddDays(-(7 + (DateTime.Today.DayOfWeek - DayOfWeek.Monday)) % 7);
        isNextDisabled = monday >= todayMonday;
        
        _ = SafeRunAsync(async () => await GetAllActivities(true));
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
    
    private async Task GetAllActivities(bool isLoading)
    {
        IsLoading = isLoading;
        StateHasChanged();
        
        try
        {
            int year = currentDate.Year;
            int weekNumber = GetWeekNumber();
            weekNumberSelected = weekNumber;
            
            DataAResponse<ActivitiesByDay> response = await ActivityWeeklyService.GetAllActivitiesPerRangeWeek(year, weekNumber);
            
            if (response.Data.Count == 0)
            {
                AllActivities = GenerateEmptyWeek(year, weekNumber);
            }
            else
            {
                AllActivities = MergeWithFullWeek(response.Data, year, weekNumber);
            }
            
            Activities = new List<ActivitiesByDay>(AllActivities);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            IsLoading = false;
            throw;
        }
        finally
        {
            IsLoading = false;
            StateHasChanged();
        }
    }
    
    private string StartTimeString
    {
        get => filterDto.timeFrom!;
        set => filterDto.timeFrom = value;
    }
    
    private string EndTimeString
    {
        get => filterDto.timeTo!;
        set => filterDto.timeTo = value;
    }
    
    private static string Plural(int count, string singular, string plural) =>
        count == 1 ? singular : plural;
    
    private async Task FilterSearch()
    {
        isLoadingFiltros = true;
        StateHasChanged();

        try
        {
            await Task.Delay(300);
            activeFiltersCount = 0;

            // empezamos con la lista completa
            IEnumerable<ActivitiesByDay> filtered = AllActivities;

            // filtro por texto (SearchTerm)
            if (!string.IsNullOrWhiteSpace(filterDto.SearchTerm))
            {
                string search = filterDto.SearchTerm.ToLower();
                activeFiltersCount++;

                filtered = filtered.Select(day => new ActivitiesByDay
                {
                    Day = day.Day,
                    DayName = day.DayName,
                    DayNameAndDay = day.DayNameAndDay,
                    Worked = day.Worked,
                    Activities = day.Activities
                        .Where(a =>
                            (!string.IsNullOrEmpty(a.Requeriment?.Titulo) && a.Requeriment.Titulo.ToLower().Contains(search)) ||
                            (!string.IsNullOrEmpty(a.Description) && a.Description.ToLower().Contains(search))
                        ).ToList()
                });
            }

            // filtro por texto (ReqID)
            if (!string.IsNullOrWhiteSpace(filterDto.reqID))
            {
                activeFiltersCount++;
                filtered = filtered.Select(b => new ActivitiesByDay
                {
                    Day = b.Day,
                    DayName = b.DayName,
                    DayNameAndDay = b.DayNameAndDay,
                    Worked = b.Worked,
                    Activities = b.Activities
                        .Where(a => a.Requeriment?.ReqID == filterDto.reqID || a.Requeriment!.ReqID.Contains(filterDto.reqID)).ToList()
                });

            }

            // filtro por estado
            if (!string.IsNullOrWhiteSpace(filterDto.estadoSel))
            {
                activeFiltersCount++;
                filtered = filtered.Select(day => new ActivitiesByDay
                {
                    Day = day.Day,
                    DayName = day.DayName,
                    DayNameAndDay = day.DayNameAndDay,
                    Worked = day.Worked,
                    Activities = day.Activities
                        .Where(a => a.StatusMessage == filterDto.estadoSel)
                        .ToList()
                });
            }

            // filtro por rango de fechas
            bool hasDateFilter = filterDto.dateFrom.HasValue || filterDto.dateTo.HasValue;
            if (hasDateFilter)
            {
                activeFiltersCount++;
                filtered = filtered
                    .Where(day =>
                    {
                        DateTime current = day.Day.ToDateTime(TimeOnly.MinValue);
                        bool inRange = true;

                        if (filterDto.dateFrom.HasValue)
                            inRange &= current >= filterDto.dateFrom.Value;
                        if (filterDto.dateTo.HasValue)
                            inRange &= current <= filterDto.dateTo.Value;

                        return inRange;
                    })
                    .Select(day => new ActivitiesByDay
                    {
                        Day = day.Day,
                        DayName = day.DayName,
                        DayNameAndDay = day.DayNameAndDay,
                        Worked = day.Worked,
                        Activities = day.Activities
                    });
            }

            
           
           if (!string.IsNullOrEmpty(filterDto.timeFrom) && !string.IsNullOrEmpty(filterDto.timeTo))
           {
               double? from = double.TryParse(filterDto.timeFrom, out double minHours) ? minHours : null;
               double? to = double.TryParse(filterDto.timeTo, out double maxHours) ? maxHours : null;

               if (from.HasValue && to.HasValue)
                   activeFiltersCount++;

               filtered = filtered.Where(day =>
               {
                   double totalHours = day.Worked.TotalHours;
                   bool valid = true;

                   if (from.HasValue)
                       valid &= totalHours >= from.Value;

                   if (to.HasValue)
                       valid &= totalHours <= to.Value;

                   return valid;
               });
           }

            // mergeamos la semana completa (mantiene días vacíos)
            Activities = MergeWithFullWeek(filtered.ToList(), currentDate.Year, GetWeekNumber());
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        finally
        {
            isLoadingFiltros = false;
            StateHasChanged();
        }
    }

    
    private List<ActivitiesByDay> GenerateEmptyWeek(int year, int weekNumber)
    {
        (DateTime start, DateTime end) = GetDateRangeFromWeek(year, weekNumber);
        List<ActivitiesByDay> list = new List<ActivitiesByDay>();
        for (DateTime date = start; date <= end; date = date.AddDays(1))
        {
            list.Add(new ActivitiesByDay
            {
                Day = DateOnly.FromDateTime(date),
                DayName = date.ToString("dddd").ToUpper(),
                DayNameAndDay = $"{CurrentCulture.TextInfo.ToTitleCase(date.ToString("dddd", new CultureInfo("es-ES")))} {date:dd/MM}",
                Worked = TimeSpan.Zero,
                Activities = new()
            });
        }
        return list;
    }

    private List<ActivitiesByDay> MergeWithFullWeek(List<ActivitiesByDay> existing, int year, int weekNumber)
    {
        List<ActivitiesByDay> fullWeek = GenerateEmptyWeek(year, weekNumber);

        foreach (ActivitiesByDay? day in fullWeek)
        {
            ActivitiesByDay? found = existing.FirstOrDefault(x => x.Day == day.Day);
            if (found != null)
            {
                day.Worked = found.Worked;
                day.Activities = found.Activities;
            }
        }
        return fullWeek;
    }

    private (DateTime start, DateTime end) GetDateRangeFromWeek(int year, int weekNumber)
    {
        DateTime jan1 = new DateTime(year, 1, 1);
        int daysOffset = DayOfWeek.Monday - jan1.DayOfWeek;
        DateTime firstMonday = jan1.AddDays(daysOffset);
        DateTime startOfWeek = firstMonday.AddDays((weekNumber - 1) * 7); //7
        DateTime endOfWeek = startOfWeek.AddDays(5); // 6
        return (startOfWeek, endOfWeek);
    }
    
    private string GetDayStatus(ActivitiesByDay? day)
    {
        if (day == null || day.Activities.Count == 0)
            return "empty";

        double totalHours = 0;

        foreach (ActivityResponseDto activity in day.Activities)
        {
            if (string.IsNullOrEmpty(activity.TimeElapsed))
                continue;

            string[] parts = activity.TimeElapsed.Split(':');
            if (parts.Length == 2 &&
                int.TryParse(parts[0], out int hours) &&
                int.TryParse(parts[1], out int minutes))
            {
                totalHours += hours + minutes / 60.0;
            }
        }


        if (Enum.TryParse<DiasSemana>(day.DayName, true, out DiasSemana diaEnum))
        {
            if (_configHoras.TryGetValue(diaEnum, out double minRequerida))
            {
                if (totalHours >= minRequerida && minRequerida > 0)
                    return "completed";
            }
        }

        return totalHours > 0 ? "partial" : "empty";
    }

    string GetStatusColor(string status)
    {
        switch (status)
        {
            case "completed":
                return "#22C55E";
            case "partial":
                return "#EAB308";
            default:    
                return "#FCA5A5";
        }
    }
    
    private Task UpdateSearch()
    {
        BuscarPor = "";
        search = "";
        SearchEmpty = false;
        StateHasChanged();
        return Task.CompletedTask;
    }
    
    private Task DoSearch(string? query)
    {
        search = query;
        filterDto.SearchTerm = query;
        return Task.CompletedTask;
    }
    #endregion
    
    #region Limpiar
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
        State.OnSidebarChanged -= StateHasChanged;
    }

    private void LimpiarFiltros()
    {
        filterDto = new()
        {
            SearchTerm = null,
            reqID = null,
            estadoSel = null,
            dateFrom = null,
            dateTo = null,
            timeFrom = null,
            timeTo = null
        };
        DoSearch(null);
        EstadoSeleccionado = "";
        activeFiltersCount = 0;
        _ = SafeRunAsync(async () => await GetAllActivities(true));
        StateHasChanged();
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
    
    #region ModalesFunciones
    private async Task ShowModal(Guid id)
    {
        await showModalRef!.ShowAsync(id);
    }
    #endregion
}