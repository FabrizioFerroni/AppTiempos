using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class BreadcrumbLink : ComponentBase
{
    [Parameter] public string? Href { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private string GetClasses()
    {
        string baseClasses =
            $"flex flex-wrap items-center gap-1.5 break-words text-sm text-muted-foreground sm:gap-2.5";

        return Cn(baseClasses, Class);
    }
}