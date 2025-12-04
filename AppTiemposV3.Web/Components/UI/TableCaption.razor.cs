using Microsoft.AspNetCore.Components;
using static AppTiemposV3.Web.Utils.CssHelper;

namespace AppTiemposV3.Web.Components.UI;

public partial class TableCaption : ComponentBase
{
    [Parameter] public string? Class { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }


    protected string GetCaptionCssClass()
    {
        string baseClasses =
            "text-muted-foreground mt-4 text-sm";

        return Cn(baseClasses, Class);

    }
}