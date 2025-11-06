using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class CalendarFilterMonth : ComponentBase, IDisposable
{
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Parameter] public DateTime CurrentMonth { get; set; }
    [Parameter] public DateTime? SelectedStart { get; set; } = DateTime.Today;
    [Parameter] public DateTime? SelectedEnd { get; set; }
    
    [Parameter] public DateTime? MinDate { get; set; }
    [Parameter] public DateTime? MaxDate { get; set; }
    [Parameter] public bool AllowFutureDates { get; set; } = false;
    [Parameter] public EventCallback<DateTime> OnDateSelected { get; set; }
    [Parameter] public EventCallback<DateTime> OnMonthChanged { get; set; }

    protected override Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
        return Task.CompletedTask;
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }

    private void OnDayClick(DateTime date) => OnDateSelected.InvokeAsync(date);

    private async Task PreviousMonth()
    {
        DateTime newMonth = CurrentMonth.AddMonths(-1);
        await OnMonthChanged.InvokeAsync(newMonth);
    }
    private async Task NextMonth()
    {
        DateTime newMonth = CurrentMonth.AddMonths(1);
        await OnMonthChanged.InvokeAsync(newMonth);
    }

    private IEnumerable<DateTime?> GetDaysForMonth()
    {
        DateTime first = new DateTime(CurrentMonth.Year, CurrentMonth.Month, 1);
        int skip = ((int)first.DayOfWeek + 6) % 7;
        IEnumerable<DateTime?> days = Enumerable.Repeat<DateTime?>(null, skip)
            .Concat(Enumerable.Range(1, DateTime.DaysInMonth(CurrentMonth.Year, CurrentMonth.Month))
                .Select(d => (DateTime?)new DateTime(CurrentMonth.Year, CurrentMonth.Month, d)));
        return days;
    }
    
    private string GetDayClass(DateTime day, bool disabled)
    {
        bool isStart = SelectedStart.HasValue && day.Date == SelectedStart.Value.Date;
        bool isEnd = SelectedEnd.HasValue && day.Date == SelectedEnd.Value.Date;
        bool inRange = SelectedStart.HasValue && SelectedEnd.HasValue &&
                       day.Date > SelectedStart.Value.Date && day.Date < SelectedEnd.Value.Date;

        if (disabled) return "text-gray-400 cursor-not-allowed opacity-50";

        if (isStart || isEnd)
            return $"{ColorService.GetButtonClassess()} text-white rounded-full px-2 py-1 ";
        if (inRange)
            return "bg-blue-100 dark:bg-blue-900/30 text-blue-700 dark:text-blue-300 rounded-full px-2 py-1";
        return "hover:bg-gray-200 dark:hover:bg-gray-700 rounded-full px-2 py-1";
    }
    
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
    }
}