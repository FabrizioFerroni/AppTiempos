using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class BreadcrumbPage : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    private string GetClasses()
    {
        string baseClasses =
            $"font-normal text-[hsl(var(--foreground))]";

        return Cn(baseClasses, Class);
    }
}