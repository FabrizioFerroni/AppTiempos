using AppTiemposV3.Web.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace AppTiemposV3.Web.Pages;

public partial class Home: ComponentBase
{
    [Inject] LayoutState State { get; set; } = null!;
    
    private void HandleSidebarToggle()
    {
        _ = State.ToggleSidebar();
    }
}