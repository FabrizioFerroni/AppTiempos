using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class SidebarTrigger : ComponentBase
{
    [Parameter] public EventCallback OnToggle { get; set; }
    [Inject] private ILocalStorageService _localStorageService { get; set; } = default!;
    private bool IsOpen = false;
    [Parameter] public bool IsSidebarClosed { get; set; }
    [Parameter]
    public string? Class { get; set; }


    private async Task OnClickHandler()
    {
        if (OnToggle.HasDelegate)
        {
            await OnToggle.InvokeAsync();
        }
    }
    
    private string GetClasses()
    {
        string baseClasses =
            $"h-7 w-7 cursor-pointer";

        return Cn(baseClasses, Class);
    }
}