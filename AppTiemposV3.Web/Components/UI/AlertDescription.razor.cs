using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class AlertDescription : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    
    
    private string GetClasses()
    {
        string baseClasses =
            $"text-sm [&_p]:leading-relaxed";

        return Cn(baseClasses, Class);
    }
}