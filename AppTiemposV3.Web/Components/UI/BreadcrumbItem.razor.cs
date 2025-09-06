using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class BreadcrumbItem : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private string GetClasses()
    {
        string baseClasses =
            $"inline-flex items-center gap-1.5";

        return Cn(baseClasses, Class);
    }
}