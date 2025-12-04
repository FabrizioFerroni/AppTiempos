using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class TableHeader : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
    
    
    protected string GetTHeadCssClass()
    {
        string baseClasses =
            "[&_tr]:border-b";

        return Cn(baseClasses, Class);
    }
}