using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class Switch : ComponentBase, IDisposable
{
    [Parameter] public bool Checked { get; set; }
    [Parameter] public EventCallback<bool> CheckedChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Id { get; set; }

    [Inject] private ColorService ColorService { get; set; } = null!;

    protected override Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
        return Task.CompletedTask;
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }


    protected async Task Toggle()
    {
        if (Disabled) return;
        Checked = !Checked;
        await CheckedChanged.InvokeAsync(Checked);
    }
    
    protected string GetButtonClasses()
    {
        string stateClasses = Checked
            ? ColorService.GetButtonClassess()
            : "bg-white dark:bg-gray-700";

        string baseClasses =
            "peer inline-flex h-6 w-11 shrink-0 cursor-pointer items-center rounded-full  " +
            "transition-colors " +
            " disabled:cursor-not-allowed disabled:opacity-50";
        
        //focus-visible:ring-offset-[hsl(var(--background))]
        //focus-visible:ring-offset-2
        //focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring
        //border-2 border-[hsl(var(--transparent))]

        return Cn(baseClasses, stateClasses, Class);
    }

    protected string GetThumbClasses()
    {
        string thumbPosition = Checked ? "translate-x-5" : "translate-x-0";
        string baseClasses =
            "pointer-events-none block h-5 w-5 rounded-full bg-white shadow-lg transition-transform";
        
        // ring-0
        //bg-[hsl(var(--background))]

        return Cn(baseClasses, thumbPosition);
    }
    
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
    }
}