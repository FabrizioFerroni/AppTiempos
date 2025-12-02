using System.Globalization;
using AppTiemposV3.SharedClases.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class CalendarFilterView : ComponentBase
{
    
    #region Variables
    protected bool IsOpen { get; set; } = false;
    [Parameter] public string? ClassButton {get; set;}
    [Parameter] public string? ClassDiv {get; set;}
    [Parameter] public string? Style {get; set;}
    [Parameter] public string? Id {get; set;}
    [Parameter] public string? With { get; set; } = "w-[350px]";
    [Parameter] public string? ClassHeight {get; set;} = string.Empty;
    [Parameter] public int weekNumber { get; set; } = 1;
    [Parameter] public CalendarMode Mode { get; set; } = CalendarMode.Single;
    [Parameter] public bool AllowFutureDates { get; set; } = false;
    [Parameter] public bool FilterByWeek { get; set; } = false;
    [Parameter] public bool Disabled { get; set; } = false;
    [Parameter] public EventCallback<DateTime?> OnDateSelected { get; set; }
    [Parameter] public EventCallback<(DateTime? Start, DateTime? End)> OnRangeSelected { get; set; }
    [Parameter] public EventCallback<bool> OnDropdownStateChanged { get; set; }
    [Parameter] public DateTime? SelectedStart { get; set; }
    protected DateTime? SelectedEnd { get; set; }
    protected DateTime CurrentMonth { get; set; } = DateTime.Today;
    
    private string DateDropdown = string.Empty;
    
    private ElementReference ElementRef;
    private string ElementId = $"calendar-filter-{Guid.NewGuid()}";
    private DateTime? MinDate;
    private DateTime? MaxDate;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private DotNetObjectReference<CalendarFilterView>? DotNetRef;
    #endregion
    
    #region Inicializacion
    protected async override Task OnInitializedAsync()
    {
        SetWeekRange();
        FechaStringDropdown();
        await base.OnInitializedAsync();
    }
    
    protected override void OnParametersSet()
    {
        SetWeekRange();
        
        if (SelectedStart < MinDate || SelectedStart > MaxDate)
            SelectedStart = (DateTime)MinDate!;

        if (Mode == CalendarMode.Range)
            SelectedEnd = MaxDate;
        
        if (SelectedStart.HasValue)
        {
            ChangeMonth(SelectedStart.Value);
        }
        else
        {
            ChangeMonth(DateTime.Today);
        }
        FechaStringDropdown(SelectedStart);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            SelectedStart = DateTime.Now;
            DotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("clickOutsideHandler.add", DotNetRef, ElementId);
        }
    }

    #endregion
    
     #region Funciones
     private void SetWeekRange()
     {
         if (!FilterByWeek)
         {
             MinDate = null;
             MaxDate = null;
             return;
         }
         
         // Calcula lunes como inicio de la semana
         DateTime firstDayOfYear = new DateTime(DateTime.Today.Year, 1, 1);
         int daysOffset = DayOfWeek.Monday - firstDayOfYear.DayOfWeek;
         DateTime firstMonday = firstDayOfYear.AddDays(daysOffset);
         DateTime start = firstMonday.AddDays((weekNumber - 1) * 7);
         DateTime end = start.AddDays(5);
         MinDate = start;
         MaxDate = end;
         StateHasChanged();
     }
     private string GetClasessButton()
     {
         string baseClasses =
             $"justify-start text-left font-normal bg-transparent cursor-pointer";

         string baseOpen = IsOpen ? " bg-gray-100 dark:bg-gray-700" : "";

         return Cn(baseClasses, baseOpen, ClassButton);
     }
     
     private string GetClasessDivCalendar()
     {
         string baseClasses = "absolute left-0 mt-2 z-50 p-4 bg-white dark:bg-gray-800 rounded-2xl shadow-lg border border-gray-200 dark:border-gray-700";

         return Cn(baseClasses, With);
     }

     private string GetClassesDiv()
     {
         string baseClasses =
             "relative inline-block";

         return Cn(baseClasses, ClassDiv);
     }
    protected void ToggleCalendar()
    {
        IsOpen = !IsOpen;
        if (OnDropdownStateChanged.HasDelegate)
        {
            OnDropdownStateChanged.InvokeAsync(!IsOpen); // true = cerrado
        }
    }
    private void FechaStringDropdown(DateTime? fecha = null)
    {
        DateTime fechaSeleccionada = fecha.GetValueOrDefault(DateTime.Today);
        CultureInfo cultura = new CultureInfo("es-ES");
        DateDropdown = fecha.HasValue
            ? fechaSeleccionada.ToString("dd/MM/yyyy", cultura)
            : "dd/mm/aaaa";
        StateHasChanged();
    }

    protected void HandleDateSelected(DateTime date)
    {
        if (Mode == CalendarMode.Single)
        {
            SelectedStart = date;
            FechaStringDropdown(SelectedStart);
            IsOpen = false;
            OnDateSelected.InvokeAsync(date);
            if (OnDropdownStateChanged.HasDelegate)
            {
                OnDropdownStateChanged.InvokeAsync(!IsOpen); // true = cerrado
            }
        }
        else
        {
            if (SelectedStart == null || (SelectedStart != null && SelectedEnd != null))
            {
                SelectedStart = date;
                SelectedEnd = null;
            }
            else if (date < SelectedStart)
            {
                SelectedEnd = SelectedStart;
                SelectedStart = date;
            }
            else
            {
                SelectedEnd = date;
            }

            OnRangeSelected.InvokeAsync((SelectedStart, SelectedEnd));
            
            if (OnDropdownStateChanged.HasDelegate)
            {
                OnDropdownStateChanged.InvokeAsync(!IsOpen); // true = cerrado
            }
        }
    }

    protected void SelectToday()
    {
        if (Mode == CalendarMode.Single)
        {
            SelectedStart = DateTime.Today;
            IsOpen = false;
            FechaStringDropdown(SelectedStart);
            OnDateSelected.InvokeAsync(DateTime.Today);
        }
        else
        {
            SelectedStart = DateTime.Today;
            SelectedEnd = DateTime.Today;
            IsOpen = false;
            FechaStringDropdown(SelectedStart);
            OnRangeSelected.InvokeAsync((SelectedStart, SelectedEnd));
        }
        ChangeMonth(DateTime.Today);
    }
    
    protected void ChangeMonth(DateTime newMonth)
    {
        CurrentMonth = newMonth;
    }
    
    [JSInvokable]
    public void CloseCalendar()
    {
        if (IsOpen)
        {
            IsOpen = false;
            OnDropdownStateChanged.InvokeAsync(!IsOpen);
            InvokeAsync(StateHasChanged);
        }
    }   
    #endregion
    
    #region Limpieza
    protected void ClearSelection()
    {
        SelectedStart = DateTime.Today;
        ChangeMonth(DateTime.Today);
        IsOpen = false;
        SelectedEnd = Mode == CalendarMode.Range ? DateTime.Today : null;
        if (Mode == CalendarMode.Single)
        {
            OnDateSelected.InvokeAsync(SelectedStart);
        }
        else
        {
            OnRangeSelected.InvokeAsync((SelectedStart, SelectedEnd));
        }
    }
    
    public async ValueTask DisposeAsync()
    {
        DotNetRef?.Dispose();
    }
    #endregion
}