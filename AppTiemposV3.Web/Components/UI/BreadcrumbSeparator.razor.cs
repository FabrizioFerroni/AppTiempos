using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class BreadcrumbSeparator : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private string GetClasses()
    {
        string baseClasses =
            $"[&>svg]:w-3.5 [&>svg]:h-3.5";

        return Cn(baseClasses, Class);
    }
}