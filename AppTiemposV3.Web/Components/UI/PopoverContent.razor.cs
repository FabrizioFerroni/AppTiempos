using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class PopoverContent : ComponentBase
{
    [CascadingParameter] public Popover? PopoverParent { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public string Class { get; set; }
    
    private string GetClasses()
    {
        string baseClasses =
            "z-50 w-72 rounded-md border bg-white p-4 shadow-md outline-none";

        return Cn(baseClasses, Class);
    }
}