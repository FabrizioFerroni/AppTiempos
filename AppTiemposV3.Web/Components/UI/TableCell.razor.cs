using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class TableCell : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }


    protected string GetTdCssClass()
    {
        string baseClasses =
            "text-foreground h-10 px-2 text-left align-middle font-medium whitespace-nowrap [&:has([role=checkbox])]:pr-0 [&>[role=checkbox]]:translate-y-[2px]";

        return Cn(baseClasses, Class);

    }
}