using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class TableRow : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }
    
    
    protected string GetTrCssClass()
    {
        string baseClasses =
            "hover:bg-[hsl(var(--muted))]/50 data-[state=selected]:bg-[hsl(var(--muted))] border-b transition-colors";

        return Cn(baseClasses, Class);
    }
}