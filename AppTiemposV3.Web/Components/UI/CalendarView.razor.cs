using System.Globalization;
using AppTiemposV3.SharedClases.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Components.UI;

public partial class CalendarView : ComponentBase
{
    #region Variables
    protected bool IsOpen { get; set; } = false;
    [Parameter] public CalendarMode Mode { get; set; } = CalendarMode.Single;
    [Parameter] public bool AllowFutureDates { get; set; } = false;
    [Parameter] public EventCallback<DateTime?> OnDateSelected { get; set; }
    [Parameter] public EventCallback<(DateTime? Start, DateTime? End)> OnRangeSelected { get; set; }
    protected DateTime? SelectedStart { get; set; }
    protected DateTime? SelectedEnd { get; set; }
    protected DateTime CurrentMonth { get; set; } = DateTime.Today;
    
    private string DateDropdown = string.Empty;
    
    private ElementReference ElementRef;
    private string ElementId = $"calendar-{Guid.NewGuid()}";
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private DotNetObjectReference<CalendarView>? DotNetRef;
    #endregion
    
    #region Inicializacion
    protected async override Task OnInitializedAsync()
    {
        FechaStringDropdown();
        await base.OnInitializedAsync();
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
    protected void ToggleCalendar()
    {
        IsOpen = !IsOpen;
    }
    private void FechaStringDropdown(DateTime? fecha = null)
    {
        DateTime fechaSeleccionada = fecha ?? DateTime.Now;
        CultureInfo? cultura = new CultureInfo("es-ES");
        DateDropdown = fechaSeleccionada.ToString("ddd, dd MMM", cultura);
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