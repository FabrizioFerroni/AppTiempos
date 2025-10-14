using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Components.UI;

public partial class NotFoundUI : ComponentBase, IDisposable
{
    [Inject] private ColorService ColorService { get; set; } = null!;
    [Inject] private IJSRuntime JS { get; set; }
    
    protected override Task OnInitializedAsync()
    {
        ColorService.OnColorChanged += HandleColorChanged;
        return Task.CompletedTask;
    }
    
    private async void HandleColorChanged()
    {
        await InvokeAsync(StateHasChanged); 
    }
    
    private void GoBack()
    {
        JS.InvokeVoidAsync("history.back");
    }
    
    public void Dispose()
    {
        ColorService.OnColorChanged -= HandleColorChanged; 
    }
}