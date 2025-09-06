using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class DropdownContent : ComponentBase
{
    [CascadingParameter] public Dropdown? DropdownParent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Class { get; set; }
    
    private void ToggleDropdown() => DropdownParent?.Toggle();
    
    private string GetClasses()
    {
        string baseClasses =
            "p-2 rounded-md hover:bg-gray-100 dark:hover:bg-gray-700";


        return Cn(baseClasses, Class);
    }
}