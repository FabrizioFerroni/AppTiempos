using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class ShowPassword : ComponentBase
{
    [Parameter] public bool IsVisible { get; set; } = false;
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }

    private async Task ToggleVisibility()
    {
        IsVisible = !IsVisible;
        await IsVisibleChanged.InvokeAsync(IsVisible);
    }
}