using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class AlertTitle : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    
    private string GetClasses()
    {
        string baseClasses =
            $"mb-1 font-medium leading-none tracking-tight";

        return Cn(baseClasses, Class);
    }
    
}