using Microsoft.AspNetCore.Components;

namespace AppTiemposV3.Web.Components.UI;

public partial class DropdownItem : ComponentBase
{
    [CascadingParameter] public Dropdown? DropdownParent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    [Parameter] public EventCallback OnClick { get; set; }

    private async Task HandleClick()
    {
        if (OnClick.HasDelegate)
            await OnClick.InvokeAsync();

        DropdownParent?.Close();
    }
}