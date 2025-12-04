using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;
namespace AppTiemposV3.Web.Components.UI;

public partial class TableFooter : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }


    protected string GetTFootCssClass()
    {
        string baseClasses =
            "bg-muted/50 border-t font-medium [&>tr]:last:border-b-0";

        return Cn(baseClasses, Class);

    }
}