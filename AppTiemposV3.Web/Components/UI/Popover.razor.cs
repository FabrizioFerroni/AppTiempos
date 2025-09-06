using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class Popover : ComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    public bool IsOpen { get; private set; }

    public void Toggle() => IsOpen = !IsOpen;
    public void Open() => IsOpen = true;
    public void Close() => IsOpen = false;
}