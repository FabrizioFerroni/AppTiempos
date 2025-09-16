using AppTiemposV3.Web.Services;
using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Layout;

public partial class AppLayout 
{
    [Inject] LayoutState State { get; set; } = default!;
    
    protected override async Task OnInitializedAsync()
    {
        State.OnSidebarChanged += StateHasChanged;
        await State.InitializeAsync();
    }

    public void Dispose()
    {
        State.OnSidebarChanged -= StateHasChanged;
    }
}