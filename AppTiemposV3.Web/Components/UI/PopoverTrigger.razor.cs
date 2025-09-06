using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class PopoverTrigger : ComponentBase
{
    [CascadingParameter] public Popover? PopoverParent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    
    private void TogglePopover() => PopoverParent?.Toggle();
}