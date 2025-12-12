using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class SearchInput : ComponentBase
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public EventCallback<string?> OnSearch { get; set; }
    [Parameter] public int Delay { get; set; } = 500;
    [Parameter] public bool Disabled { get; set; }
    private string? currentValue;
    private System.Timers.Timer? debounceTimer;
    
    protected override void OnParametersSet()
    {
        // Sincronizar el valor externo con el interno
        if (Value != currentValue)
            currentValue = Value;
    }
    
    /*private void OnValueChanged(string? value)
    {
        currentValue = value;

        // resetear timer
        debounceTimer?.Stop();
        debounceTimer?.Dispose();

        debounceTimer = new System.Timers.Timer(Delay);
        debounceTimer.Elapsed += async (_, __) =>
        {
            debounceTimer?.Stop();
            debounceTimer?.Dispose();

            await InvokeAsync(() => OnSearch.InvokeAsync(currentValue));
        };
        debounceTimer.AutoReset = false;
        debounceTimer.Start();
    }*/
    
    private async void OnValueChanged(string? value)
    {
        currentValue = value;

        // Notificar al padre que cambió el valor
        await ValueChanged.InvokeAsync(currentValue);

        // debounce
        debounceTimer?.Stop();
        debounceTimer?.Dispose();
        debounceTimer = new System.Timers.Timer(Delay);
        debounceTimer.Elapsed += async (_, __) =>
        {
            debounceTimer?.Stop();
            debounceTimer?.Dispose();
            await InvokeAsync(() => OnSearch.InvokeAsync(currentValue));
        };
        debounceTimer.AutoReset = false;
        debounceTimer.Start();
    }
}